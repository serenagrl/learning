// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using System;

namespace Consumer
{
    class DirectConsumer : IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
        private DefaultBasicConsumer _consumer;

        public string Hostname { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }
        public string Queue { get; }

        public DirectConsumer(string exchange, string routingKey,
            string hostname = "localhost", string userName = ConnectionFactory.DefaultUser, 
            string password = ConnectionFactory.DefaultPass)
        {
            this.Hostname = hostname;
            this.Exchange = exchange;
            this.RoutingKey = routingKey;

            var factory = new ConnectionFactory()
            {
                HostName = hostname,
                UserName = userName,
                Password = password,
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Uses Direct Exchange.
            _channel.ExchangeDeclare(exchange, ExchangeType.Direct, autoDelete: true);
            _channel.BasicQos(0, 1, false);

            // RabbitMQ will generate own queue name for us.
            this.Queue = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(this.Queue, exchange, routingKey);

            _consumer = new PastryReceiver(_channel);
        }

        public void Consume()
        {
            _channel.BasicConsume(this.Queue, false, _consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
