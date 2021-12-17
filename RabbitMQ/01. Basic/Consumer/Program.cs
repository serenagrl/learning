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

namespace Consumer
{
    // Learned from here -> https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html
    class Program
    {
        private const string QUEUE_NAME = "learning.basic";
        static void Main(string[] args)
        {
            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: QUEUE_NAME,
                                 durable: false, 
                                 exclusive: false, 
                                 autoDelete: true); // Queue will be gone after this app shutdown.

            // Can also specify like this but the above look more descriptive.
            //channel.QueueDeclare(QUEUE_NAME, false, false, true);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, args) =>
            {
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received {message}");
            };

            channel.BasicConsume(queue: QUEUE_NAME,
                                 autoAck: true,
                                 consumer: consumer);

            // Can also specify like this but the above look more descriptive.
            //channel.BasicConsume(QUEUE_NAME, true, consumer);

            Console.ReadKey();
        }


    }
}
