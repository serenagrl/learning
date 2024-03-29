﻿// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Consumer
{
    class Program
    {
        private const string EXCHANGE_NAME = "learning.deadletter.headers";
        private const string QUEUE_NAME_PREFIX = "learning.deadletter.headers";
        private const string RETRY_EXCHANGE_NAME = "learning.deadletter.headers.retry";
        private const string RETRY_QUEUE_NAME = "learning.deadletter.headers.retry";
        private const string RETRY_ROUTING_KEY = "retry.key";
        private const int RETRY_INTERVAL_MS = 5000; // set this longer to keep it inside the retry queue
        private const int MAX_RETRIES = 3;

        private static readonly string[] PASTRIES = { "Honeybun", "Sugarplum" };

        static void Main(string[] args)
        {
            string routingKey = GetSelection();

            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Define headers
            var headers = new Dictionary<string, object>();
            headers.Add("x-match", "any");
            headers.Add("Pastry", routingKey);

            // Main exchange and queue.
            string queueName = SetupMainExchangeAndQueue(routingKey, channel, headers);

            // Retry exchange and queue.
            SetupRetryExchangeAndQueue(channel, headers);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(queueName, false, consumer);
            Console.ReadKey();
        }
        
        private static string SetupMainExchangeAndQueue(string pastry, IModel channel, Dictionary<string, object> headers)
        {
            // Declare retry exchange properties.
            var arguments = new Dictionary<string, object>()
            {
                { "x-dead-letter-exchange", RETRY_EXCHANGE_NAME },
                { "x-dead-letter-routing-key", RETRY_ROUTING_KEY },
                { "x-message-ttl", 30000 }
            };

            string queueName = $"{QUEUE_NAME_PREFIX}.{pastry}";

            // Declage and bind Main queue to Main exchange.
            SetupExchangeAndQueue(channel, EXCHANGE_NAME, queueName, headers, arguments);

            return queueName;
        }

        private static void SetupRetryExchangeAndQueue(IModel channel, Dictionary<string, object> headers)
        {
            // Retry Exchange properties to route back to main exchange
            var arguments = new Dictionary<string, object>()
            {
                { "x-dead-letter-exchange", EXCHANGE_NAME },
                { "x-dead-letter-routing-key", RETRY_ROUTING_KEY },
                { "x-message-ttl", RETRY_INTERVAL_MS } 
            };

            // Declare and bind retry Exchange and retry queue.
            SetupExchangeAndQueue(channel, RETRY_EXCHANGE_NAME, RETRY_QUEUE_NAME, headers, arguments);
        }

        private static void SetupExchangeAndQueue(IModel channel, string exchangeName, string queueName,
            Dictionary<string, object> headers, Dictionary<string, object> arguments)
        {
            channel.ExchangeDeclare(exchangeName, ExchangeType.Headers, autoDelete: true);

            // Note: Can change autoDelete to true to keep the queue to hold offline retries.
            channel.QueueDeclare(queueName, true, false, true, arguments);
            channel.QueueBind(queueName, exchangeName, string.Empty, headers);
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            // Get back the channel.
            IModel channel = (sender as EventingBasicConsumer)?.Model;

            // Get back the pastry value to show.
            string pastry = Encoding.UTF8.GetString(e.BasicProperties.Headers["Pastry"] as byte[]);
            
            // Check header for retries.
            int retries = ReadHeaderForRetries(e);

            DisplayAction(retries, pastry, message);

            try
            {
                // Simulate processing/error.
                ProcessWithRandomError();

                // Manual acknowledge.
                channel.BasicAck(e.DeliveryTag, false);
                Console.WriteLine($"successful!");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"randomly rejected!");

                if (retries < MAX_RETRIES)
                {
                    // Reject message.
                    channel.BasicReject(e.DeliveryTag, false);
                }
                else
                {
                    // Drop the message after reaching the max retry.
                    // Note: May want to insert this into a poison queue.
                    channel.BasicAck(e.DeliveryTag, false);
                    
                    Console.WriteLine($"[{pastry}] {message} reached maximum {MAX_RETRIES} retries. Message will be dropped.");
                }

                Console.ResetColor();

            }
        }

        private static int ReadHeaderForRetries(BasicDeliverEventArgs e)
        {
            int retries = 0;

            // Read the x-death header for retries.
            if (e.BasicProperties.Headers.ContainsKey("x-death"))
            {
                // It returns a List of objects which are Dictionary of string key and object value.
                var xdeath = ((List<object>)e.BasicProperties.Headers["x-death"]).Cast<Dictionary<string, object>>().ToList();
                // Read the latest retry count.
                retries = Convert.ToInt32(xdeath[0]["count"]);
            }

            return retries;
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

        private static void DisplayAction(int retries, string routingKey, string message)
        {
            if (retries > 0)
                Console.Write($"Retry: {retries} [");
            else
                Console.Write($"Processing [");

            Console.ForegroundColor = _keyColor;
            Console.Write($"{routingKey}");
            Console.ResetColor();
            Console.Write($"] {message}... ");
        }

        private static void ProcessWithRandomError()
        {
            // Simulate error.
            var random = new Random();
            if (random.Next(1, 50) % 5 > 0)
                throw new ApplicationException("Simulated exception");
        }

        #endregion
    }
}
