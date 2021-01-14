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
    // Learned from here -> https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html
    class Program
    {
        private const string QUEUE_NAME = "learning.basic";
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

            channel.QueueDeclare(queue:QUEUE_NAME, 
                                 durable: false, 
                                 exclusive: false, 
                                 autoDelete: true); // Queue will be gone after this app shutdown.

            // Can also specify like this but the above look more descriptive.
            //channel.QueueDeclare(QUEUE_NAME, false, false, true);

            string message = "Hello RabbitMQ";
            byte[] body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", 
                                 routingKey: QUEUE_NAME, 
                                 body: body);

            // Can also specify like this but the above look more descriptive.
            //channel.BasicPublish("", QUEUE_NAME, null, body);

            Console.WriteLine($"Sent {message}");
            Console.ReadKey();
        }
    }
}
