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
    class DirectProducer : IDisposable
    {
        private IConnection _connection;
        private IModel _channel;

        public string Hostname { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }
        public string Queue { get; }

        public DirectProducer(string hostname, string userName, string password,
            string exchange)
        {
            this.Hostname = hostname;
            this.Exchange = exchange;

            var factory = new ConnectionFactory()
            {
                HostName = hostname,
                UserName = userName,
                Password = password,
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Uses Direct Exchange
            _channel.ExchangeDeclare(exchange, ExchangeType.Direct, autoDelete: true);

        }

        public void Publish(string routingKey, string message)
        {
            byte[] body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(Exchange, routingKey, null, body);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
