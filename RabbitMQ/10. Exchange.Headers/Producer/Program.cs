// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Producer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.exchange.headers";
        private static readonly string[] PASTRIES = { "Honeybun", "Cuppycake" };
        private static readonly string[] FLAVOURS = { "Chocolate", "Vanilla" };

        static void Main(string[] args)
        {
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

            while (true)
            {
                // Get how many times to send messages or quit.
                if (!GetNoOfTimesToSend(out int times)) break;

                for (int i = 1; i <= times; i++)
                {
                    // Assemble the message.
                    string message = $"Request #{i} - {DateTime.Now}";
                    byte[] body = Encoding.UTF8.GetBytes(message);

                    var headers = new Dictionary<string, object>();
                    //Randomly pick 1.
                    headers.Add("Flavour", $"{FLAVOURS[(new Random().Next(1, 100) % FLAVOURS.Length)]}");
                    headers.Add("Pastry", $"{PASTRIES[(new Random().Next(1, 100) % PASTRIES.Length)]}");

                    // Set headers
                    var properties = channel.CreateBasicProperties();
                    properties.Headers = headers;

                    // Publish to exchange.
                    channel.BasicPublish(EXCHANGE_NAME, string.Empty, properties, body);
                    Console.WriteLine($"Sent [{headers["Flavour"]}:{headers["Pastry"]}] {message}");
                }

                Console.WriteLine($"{times} messages sent!");
            }
        }

        #region Unrelated stuff
        private static bool GetNoOfTimesToSend(out int times)
        {
            Console.Write("Enter number of messages to send or [Q] to quit: ");

            string input = Console.ReadLine();
            bool quit = (input.ToUpper() == "Q");

            times = 0;
            if (!quit && !int.TryParse(input, out times))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid number of messages.");
                Console.ResetColor();
            }

            return !quit;
        }
        #endregion
    }
}
