// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    // Refer here https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html
    public class NumberAgent : IDisposable
    {
        private const string QUEUE_NAME = "learning.rpc";
        
        private IConnection _connection;
        private IModel _channel;
        private IBasicProperties _properties;

        private string _replyQueueName;
        private EventingBasicConsumer _consumer;
        private string _clientId;

        private static readonly BlockingCollection<string> _responseQueue = new BlockingCollection<string>();

        public NumberAgent()
        {
            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            // Initialization.
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Get the dynamically created queue name.
            _replyQueueName = _channel.QueueDeclare().QueueName;

            // Set the properties.
            string correlationId = Guid.NewGuid().ToString();
            _properties = _channel.CreateBasicProperties();
            _properties.CorrelationId = correlationId; // Not sure if this should be different each time.
            _properties.ReplyTo = _replyQueueName;

            // Initialize a consumer.
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, e) =>
            {
                byte[] body = e.Body.ToArray();
                string response = Encoding.UTF8.GetString(body);

                if (e.BasicProperties.CorrelationId == correlationId)
                {
                    _responseQueue.Add(response);
                }
            };

            _clientId = Guid.NewGuid().ToString();
        }

        public int GetNumber()
        {
            string message = $"Client Id: {_clientId}";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Do Rpc RabbitMQ style.
            _channel.BasicPublish(string.Empty, QUEUE_NAME, _properties, body);
            _channel.BasicConsume(_replyQueueName, true, _consumer);

            return Convert.ToInt32(_responseQueue.Take());
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}
