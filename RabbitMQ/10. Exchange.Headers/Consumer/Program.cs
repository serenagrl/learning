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
        private const string EXCHANGE_NAME = "learning.exchange.headers";
        private static readonly string[] PASTRIES = { "Honeybun", "Cuppycake" };

        static void Main(string[] args)
        {
            string pastry = GetSelection();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "p@ssw0rd"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Use exchange instead of queue.
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Headers, autoDelete: true);

            var headers = new Dictionary<string, object>();
            headers.Add("x-match", "any");
            headers.Add("Pastry", pastry);

            // RabbitMQ will generate own queue name for us.
            string queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, EXCHANGE_NAME, string.Empty, headers);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(queueName, true, consumer);
            Console.ReadKey();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            // Extract headers.
            string flavour = Encoding.UTF8.GetString(e.BasicProperties.Headers["Flavour"] as byte[]);
            string product = Encoding.UTF8.GetString(e.BasicProperties.Headers["Pastry"] as byte[]);
            Console.Write($"Received: [{flavour}:");
            Console.ForegroundColor = _keyColor;
            Console.Write($"{product}");
            Console.ResetColor();
            Console.WriteLine($"] {message} ");
        }

        #region Unrelated stuff
        private static ConsoleColor _keyColor;

        private static string GetSelection()
        {
            Console.WriteLine("Launch two Consumer instances with different selections to test.");

            int choice = 0;
            do
            {
                Console.WriteLine("Which messages should this consumer process?\n");
                for (int i = 0; i < PASTRIES.Length; i++)
                {
                    Console.WriteLine($"  [{i + 1}] {PASTRIES[i]}");
                }

                Console.Write("\nEnter selection: ");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out choice) && (choice < 1 || choice > 2))
                    Console.WriteLine("Error: Invalid choice. Please choose again.");

            } while (choice <= 0 || choice > PASTRIES.Length);

            string routingKey = PASTRIES[choice - 1];

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
