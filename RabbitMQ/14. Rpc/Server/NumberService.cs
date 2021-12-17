// ==================================================================================
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
using System.Threading.Tasks;

namespace Server
{
    // Refer here https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html
    public class NumberService : IDisposable
    {
        private const string QUEUE_NAME = "learning.rpc";

        private int _counter = 1;
        private readonly object _incrementLock = new object();

        private IConnection _connection;
        private IModel _channel;

        public void Start()
        {
            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            // Initialization.
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declaring and setting queue properties.
            _channel.QueueDeclare(QUEUE_NAME, false, false, true);
            _channel.BasicQos(0, 1, false);

            // Start listening.
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(QUEUE_NAME, false, consumer);

            consumer.Received += Consumer_Received;

        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string request = Encoding.UTF8.GetString(body);

            var properties = e.BasicProperties;

            IModel channel = (sender as EventingBasicConsumer)?.Model;
            var replyProperties = channel.CreateBasicProperties();

            // Compare correlationId
            replyProperties.CorrelationId = properties.CorrelationId;

            lock (_incrementLock)
            {
                // Reply back to consumer.
                var response = Encoding.UTF8.GetBytes(_counter.ToString());
                channel.BasicPublish(string.Empty, properties.ReplyTo, replyProperties, response);
                channel.BasicAck(e.DeliveryTag, false);

                // Do some fancy display.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Issued number {_counter} to:");
                Console.ResetColor();
                Console.WriteLine($"  {request}\n  CorrelationId: {properties.CorrelationId}");

                _counter++;

            };
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}
