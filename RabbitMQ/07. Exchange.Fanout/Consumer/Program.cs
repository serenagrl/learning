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
    class Program
    {
        private const string EXCHANGE_NAME = "learning.exchange.fanout";

        static void Main(string[] args)
        {
            Console.WriteLine("Launch two or more Consumer instances to test.");
            Console.WriteLine("Consumer started. Waiting to process messages...");

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "p@ssw0rd"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Use exchange instead of queue.
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Fanout, autoDelete:true);

            // RabbitMQ will generate own queue name for us.
            string queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, EXCHANGE_NAME,string.Empty);
                        
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(queueName, true, consumer);
            Console.ReadKey();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received {message} ");
        }
    }
}
