// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Consumer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.exchange.topic";
        private static readonly string[] ROUTING_KEYS = { "Chocolate.*", "Vanilla.*", "*.Honeybun", "*.Cuppycake" };

        static void Main(string[] args)
        {
            string routingKey = GetSelection();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "p@ssw0rd"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Use exchange instead of queue.
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, autoDelete: true);

            // RabbitMQ will generate own queue name for us.
            string queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, EXCHANGE_NAME, routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(queueName, true, consumer);
            Console.ReadKey();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            Console.Write($"Received: [");
            Console.ForegroundColor = _keyColor;
            Console.Write($"{e.RoutingKey}");
            Console.ResetColor();
            Console.WriteLine($"] {message} ");
        }

        #region Unrelated stuff
        private static ConsoleColor _keyColor;

        private static string GetSelection()
        {
            Console.WriteLine("Launch two or more Consumer instances with different topics to test.");

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
        #endregion
    }
}
