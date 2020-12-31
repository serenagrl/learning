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
        private const string TOPIC_NAME = "learning.manual.commit";
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
                EnableAutoCommit = false,
                EnablePartitionEof = true,
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(TOPIC_NAME);

            int count = 0;
            try
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    var result = consumer.Consume(tokenSource.Token);

                    if (result.IsPartitionEOF)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Reached the end of partition {result.Partition.Value}. Waiting for more messages...");
                        Console.ResetColor();
                        continue;
                    }

                    PrintResult(result);

                    try
                    {
                        // Simulate processing
                        var random = new Random();
                        int dots = random.Next(1, 5);
                        for (int i = 0; i < dots; i++)
                        {
                            Console.Write(".");
                            Thread.Sleep(1000);
                        }

                        // Simulate error
                        if (random.Next(1, 50) % 3 == 0)
                            throw new ApplicationException("Simulated exception");

                        Console.WriteLine("done.");
                        consumer.Commit(result);

                    }
                    catch (ApplicationException)
                    {
                        // Reset the position of the offset to the errorneous record.
                        // Warning: This is only good for intermittent infrastructure failures
                        //          i.e. network interruptions, database timeouts.
                        //          This will result in an endless loop if you are rejecting based
                        //          on some business rules.
                        consumer.Seek(result.TopicPartitionOffset);

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("error.");
                        Console.ResetColor();
                    }

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
            Console.Write("Processing message at ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{result.Offset.Value} ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("in Partition ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{ result.Partition.Value}");
            Console.ResetColor();
            Console.Write($": {result.Message.Timestamp.UtcDateTime} - {result.Message.Key} - {result.Message.Value} ");
        }

    }
}
