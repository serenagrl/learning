// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using System;
using System.Text;

namespace Consumer
{
    /// <summary>
    /// Custom consumer class.
    /// </summary>
    class PastryReceiver : DefaultBasicConsumer
    {
        private IModel _channel;

        public PastryReceiver(IModel channel) => _channel = channel;

        public IModel Channel { get; }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, 
            bool redelivered, string exchange, string routingKey, IBasicProperties properties, 
            ReadOnlyMemory<byte> body)
        {
            string message = Encoding.UTF8.GetString(body.ToArray());

            Console.WriteLine($"Received: [{routingKey}] {message} ");
            _channel.BasicAck(deliveryTag, false);
        }
    }
}
