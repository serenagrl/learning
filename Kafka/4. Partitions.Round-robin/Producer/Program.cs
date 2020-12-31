// ==================================================================================
// Developed by Serena Yeoh - December 2020
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;

namespace Producer
{
    class Program
    {
        private const string BROKER = "localhost:9092";
        private const string TOPIC_NAME = "learning.partitions.round-robin";
        private const int NUMBER_OF_PARTITIONS = 2;

        static void Main(string[] args)
        {
            // Create the topic with 2 partitions.
            // Note: The number of consumers available should match the number of partitions.
            //       At the point of learning, there is no way to automatically create 2 partitions
            //       from Producer, so need to use the Admin instead.
            CreateTopic(TOPIC_NAME, NUMBER_OF_PARTITIONS);

            // Create Producer.
            var config = new ProducerConfig() 
            { 
                BootstrapServers = BROKER,
                ClientId = $"{TOPIC_NAME}.producer"
            };
            using var producer = new ProducerBuilder<Null, string>(config).Build();

            while (true)
            {
                Console.Write("Enter number of messages to send or [Q] to quit: ");

                var input = ReadInput();
                if (input.Quit) break;

                for (int i = 1; i <= input.Times; i++)
                {
                    string content = $"{DateTime.Now} - Task Request #{i}";
                    var message = new Message<Null, string>()
                    {
                        Value = content
                    };

                    // Ensure that messages are evenly distributed between partitions.
                    // Note: Somehow the default didn't work, regardless of using null keys.
                    var topicPartition = new TopicPartition(TOPIC_NAME,
                        new Partition(i % NUMBER_OF_PARTITIONS));

                    // Send the message.
                    producer.Produce(topicPartition, message);

                    PrintMessage($"{message.Value}");
                }

                producer.Flush(TimeSpan.FromSeconds(10));

                Console.WriteLine($"{input.Times} messages sent");
            }
        }

        private static (int Times, bool Quit) ReadInput()
        {
            string input = Console.ReadLine();
            bool quit = (input.ToUpper() == "Q");

            int times = 0;
            if (!quit && !int.TryParse(input, out times))
                Console.WriteLine("Error: Invalid number of messages.");

            return (times, quit);
        }

        private static void PrintMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Sent message: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{message}");
        }

        private static void CreateTopic(string topic, int partitions)
        {
            var adminConfig = new AdminClientConfig() { BootstrapServers = BROKER };
            var admin = new AdminClientBuilder(adminConfig).Build();

            try
            {
                // Note: This throws an exception if the topic already exists and there is 
                //       no way to suppress it in the API. Therefore, will just catch and
                //       ignore the exception. Did not want to go down the query first path
                //       as that will impact performance.
                admin.CreateTopicsAsync(new TopicSpecification[]
                    {
                    new TopicSpecification()
                    {
                        Name = topic,
                        ReplicationFactor = 1,
                        NumPartitions = partitions
                    }
                    }).Wait();
            }
            catch(Exception ex) when (ex.Message.Contains($"Topic '{topic}' already exists"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Topic {topic} already exists. Skipping creation.");
                Console.ResetColor();
            }
        }
    }
}
