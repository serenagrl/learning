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
        private const string QUEUE_NAME = "learning.priority.requeue";
        static void Main(string[] args)
        {
            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare queue priority.
            var arguments = new Dictionary<string, object>();
            arguments.Add("x-max-priority", 10);

            // Note: Queue set to auto delete when exit. Change to true if want to keep it.
            channel.QueueDeclare(QUEUE_NAME, false, false, true, arguments);

            // Tells RabbitMQ to save message to disk as soon as it can.
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            while (true)
            {
                // Get how many times to send messages or quit.
                if (!GetNoOfTimesToSend(out int times)) break;

                for (int i = 1; i <= times; i++)
                {
                    // Assemble the message.
                    string message = $"Request #{i} - {DateTime.Now}";
                    byte[] body = Encoding.UTF8.GetBytes(message);

                    // Ramdomize the priority and set it.
                    properties.Priority = Convert.ToByte((new Random()).Next(1, 10));

                    channel.BasicPublish(string.Empty, QUEUE_NAME, properties, body);
                    Console.WriteLine($"Sent Priority {properties.Priority} - {message}");
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
