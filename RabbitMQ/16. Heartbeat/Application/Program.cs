// ==================================================================================
// Developed by Serena Yeoh - June 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() 
            { 
                RequestedHeartbeat = TimeSpan.FromSeconds(30)
            };

            // Enable automatic recovery. Defaults to true if not set.
            factory.AutomaticRecoveryEnabled = true;

            using var connection = factory.CreateConnection();

            // Detect and Handle lost connection.
            connection.ConnectionShutdown += (sender, e) =>
            {
                Console.WriteLine("\nConnection to RabbitMQ lost.");
                Console.WriteLine($"Code: { e.ReplyCode }\nReason: { e.ReplyText }");
                Console.WriteLine("\nRestart RabbitMQ Host to detect connection recovery.");
            };

            // Detect connection recovery.
            var recovery = connection as IAutorecoveringConnection;
            recovery.RecoverySucceeded += (sender, e) =>
            {
                Console.WriteLine("\nConnection to RabbitMQ re-established.");
                Console.WriteLine("Test Successful!");
            };

            Console.WriteLine("Shutdown RabbitMQ Host to detect connection lost.");
            Console.ReadKey();

            connection.Close();

        }

    }
}
