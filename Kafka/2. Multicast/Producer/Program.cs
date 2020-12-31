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
        private const string TOPIC_NAME = "learning.multicast";
        private const int NUMBER_OF_PARTITIONS = 2;

        static void Main(string[] args)
        {
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

                    // Send the message.
                    producer.Produce(TOPIC_NAME, message);

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
    }
}