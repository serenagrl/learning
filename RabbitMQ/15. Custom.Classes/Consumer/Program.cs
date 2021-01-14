// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using System;

namespace Consumer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.custom.classes";
        private static readonly string[] ROUTING_KEYS = { "Honeybun", "Sugarplum" };

        static void Main(string[] args)
        {
            string routingKey = GetSelection();

            using var channel = new DirectConsumer("localhost", "guest","p@ssw0rd",
                EXCHANGE_NAME, routingKey);

            channel.Consume();
            Console.ReadKey();
        }

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

            Console.WriteLine($"Consumer started. Waiting to process {routingKey} messages...");

            return routingKey;
        }

    }
}
