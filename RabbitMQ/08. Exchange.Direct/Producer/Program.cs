// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using System;
using System.Text;

namespace Producer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.exchange.direct";
        private static readonly string[] ROUTING_KEYS = { "Honeybun", "Sugarplum" };

        static void Main(string[] args)
        {
            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Use exchange instead of queue.
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Direct, autoDelete: true);

            while (true)
            {
                // Get how many times to send messages or quit.
                if (!GetNoOfTimesToSend(out int times)) break;

                for (int i = 1; i <= times; i++)
                {
                    // Assemble the message.
                    string message = $"Request #{i} - {DateTime.Now}";
                    byte[] body = Encoding.UTF8.GetBytes(message);

                    //Randomly pick 1.
                    string routingKey = ROUTING_KEYS[(new Random().Next(1, 100) % ROUTING_KEYS.Length)];

                    // Publish to exchange.
                    channel.BasicPublish(EXCHANGE_NAME, routingKey, null, body);
                    Console.WriteLine($"Sent [{routingKey}] {message}");
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
