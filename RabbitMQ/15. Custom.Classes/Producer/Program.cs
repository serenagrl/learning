// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using System;

namespace Producer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.custom.classes";
        private static readonly string[] ROUTING_KEYS = { "Honeybun", "Sugarplum" };

        static void Main(string[] args)
        {
            using var channel = new DirectProducer("localhost", "guest", "p@ssw0rd", EXCHANGE_NAME);

            while (true)
            {
                // Get how many times to send messages or quit.
                if (!GetNoOfTimesToSend(out int times)) break;

                for (int i = 1; i <= times; i++)
                {
                    // Assemble the message.
                    string message = $"Request #{i} - {DateTime.Now}";

                    //Randomly pick 1.
                    string routingKey = ROUTING_KEYS[(new Random().Next(1, 100) % ROUTING_KEYS.Length)];

                    // Publish to exchange.
                    channel.Publish(routingKey, message);
                    Console.WriteLine($"Sent [{routingKey}] {message}");
                }

                Console.WriteLine($"{times} messages sent");
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
