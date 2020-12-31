// ==================================================================================
// Developed by Serena Yeoh - December 2020
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using Confluent.Kafka;
using System;
using System.Threading;

namespace Consumer
{
    class Program
    {
        private const string BROKER = "localhost:9092";
        private const string TOPIC_NAME = "learning.partitions.key";
        static void Main(string[] args)
        {
            Console.WriteLine("Consumer started. Waiting to process messages. [Ctrl+C] to stop.");
            Console.WriteLine("Note: If this is the only consumer, you need to launch another one.");

            var tokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, args) =>
            {
                args.Cancel = true;
                tokenSource.Cancel();
            };

            var config = new ConsumerConfig()
            {
                BootstrapServers = BROKER,
                GroupId = $"{TOPIC_NAME}.consumers",
                ClientId = $"{TOPIC_NAME}.consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = false,
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(TOPIC_NAME);

            int count = 0;
            try
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    var result = consumer.Consume(tokenSource.Token);
                    PrintResult(result);
                }
            }
            catch (OperationCanceledException) 
            {
                Console.WriteLine($"Consumer termination request received.");
            }
            finally
            {
                Console.WriteLine($"{count} messages processed.");
            }

            consumer.Unsubscribe();
            consumer.Close();

        }

        private static void PrintResult(ConsumeResult<string, string> result)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Consumed message at ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{result.Offset.Value} ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("in Partition ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{ result.Partition.Value}");
            Console.ResetColor();
            Console.WriteLine($": {result.Message.Timestamp.UtcDateTime} - {result.Message.Key} - {result.Message.Value} ");
        }

    }
}
