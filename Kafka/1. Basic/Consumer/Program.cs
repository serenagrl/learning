// ==================================================================================
// Developed by Serena Yeoh - December 2020
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using Confluent.Kafka;
using System;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConsumerConfig()
            {
                BootstrapServers = "localhost:9092",
                GroupId = "learning.basic.consumers",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true,
            };

            using var consumer = new ConsumerBuilder<Null, string>(config).Build();
            consumer.Subscribe("learning.basic");

            var result = consumer.Consume();

            Console.WriteLine($"Consumed message: {result.Message.Value}");
            Console.ReadKey();

            consumer.Unsubscribe();
            consumer.Close();

        }
    }
}
