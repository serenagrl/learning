// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Consumer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.deadletter";
        private const string RETRY_EXCHANGE_NAME = "learning.deadletter.retry";
        private const int RETRY_INTERVAL_MS= 5000;
        private const string QUEUE_NAME_PREFIX = "learning.deadletter";

        private static readonly string[] ROUTING_KEYS = { "Honeybun", "Sugarplum" };

        // Use to keep failed messages. Temporary solution, don't follow.
        // There is a better solution in later sample.
        private static Dictionary<string, int> _failedMessages;

        static void Main(string[] args)
        {
            _failedMessages = new Dictionary<string, int>();

            string routingKey = GetSelection();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "p@ssw0rd"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Main exchange.
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Direct, autoDelete:true);

            // Retry exchange properties.
            var arguments = new Dictionary<string, object>()
            {
                { "x-dead-letter-exchange", RETRY_EXCHANGE_NAME },
                { "x-dead-letter-routing-key", routingKey },
            };

            // Main queue.
            // Note: Dynamic queue names will cause duplicates if multiple clients are running.
            string queueName = channel.QueueDeclare($"{QUEUE_NAME_PREFIX}.{routingKey}",
               false, false, true, arguments).QueueName;

            // Bind queue to exchange with routing key.
            channel.QueueBind(queueName, EXCHANGE_NAME, routingKey);

            // Route back settings to main exchange.
            var retryArguments = new Dictionary<string, object>()
            {
                { "x-dead-letter-exchange", EXCHANGE_NAME },
                { "x-dead-letter-routing-key", routingKey },
                { "x-message-ttl", RETRY_INTERVAL_MS }
            };

            // Retry exchange.
            channel.ExchangeDeclare(RETRY_EXCHANGE_NAME, ExchangeType.Direct, autoDelete: true);

            // Retry queue.
            // Note: Queue may not auto delete.
            string retryQueueName = channel.QueueDeclare($"{queueName}.retry",
                false, false, true, retryArguments).QueueName;

            // Bind queue to exchange with routing key.
            channel.QueueBind(retryQueueName, RETRY_EXCHANGE_NAME, routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(queueName, false, consumer);
            Console.ReadKey();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            IModel channel = (sender as EventingBasicConsumer)?.Model;
            int count = CheckForRetries(e.RoutingKey, message);

            try
            {
                // Simulate processing/error.
                ProcessWithRandomError();

                // Manual acknowledge.
                channel.BasicAck(e.DeliveryTag, false);
                Console.WriteLine($"successful!");

                _failedMessages.Remove(message);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"randomly rejected!");
                Console.ResetColor();

                // Keep track of the failed messages like a n00b.
                count++;
                _failedMessages[message] = count;

                // Reject message.
                channel.BasicReject(e.DeliveryTag, false);
            }
        }

        #region Unrelated stuff
        private static ConsoleColor _keyColor;

        private static string GetSelection()
        {
            Console.WriteLine("Launch two Consumer instances with different routing key to test.");

            int choice = 0;
            do
            {
                Console.WriteLine("Which messages should this consumer process?\n");
                for (int i = 0; i < ROUTING_KEYS.Length; i++)
                {
                    Console.WriteLine($"  [{i + 1}] {ROUTING_KEYS[i]}");
                }

                Console.Write("\nEnter selection: ");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out choice) && (choice < 1 || choice > 2))
                    Console.WriteLine("Error: Invalid choice. Please choose again.");

            } while (choice <= 0 || choice > ROUTING_KEYS.Length);

            string routingKey = ROUTING_KEYS[choice - 1];

            _keyColor = (ConsoleColor)(new Random()).Next(8, 14);

            Console.Write("Consumer started. Waiting to process ");
            Console.ForegroundColor = _keyColor;
            Console.Write($"{routingKey}");
            Console.ResetColor();
            Console.WriteLine(" messages...");

            return routingKey;
        }

        private static int CheckForRetries(string routingKey, string message)
        {
            if (_failedMessages.TryGetValue(message, out int count))
                Console.Write($"Retry: {count} [");
            else
                Console.Write($"Processing [");

            Console.ForegroundColor = _keyColor;
            Console.Write($"{routingKey}");
            Console.ResetColor();
            Console.Write($"] {message}... ");

            return count;
        }

        private static void ProcessWithRandomError()
        {
            // Simulate error.
            var random = new Random();
            if (random.Next(1, 50) % 3 == 0)
                throw new ApplicationException("Simulated exception");
        }

        #endregion
    }
}
