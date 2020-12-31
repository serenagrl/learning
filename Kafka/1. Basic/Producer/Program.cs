// ==================================================================================
// Developed by Serena Yeoh - December 2020
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using Confluent.Kafka;
using System;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092",
            };

            using var producer = new ProducerBuilder<Null, string>(config).Build();

            string content = $"Hello Kafka {DateTime.Now}";
            var message = new Message<Null, string>()
            {
                Value = content
            };

            producer.Produce("learning.basic", message);
            producer.Flush(TimeSpan.FromSeconds(10));

            Console.WriteLine($"Sent {content}");
            Console.ReadKey();
        }
    }
}
