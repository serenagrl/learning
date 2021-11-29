// ==================================================================================
// Developed by Serena Yeoh - November 2021
// Disclaimer: 
//   I wrote this for my self-learning and some parts may not be that accurate.
//   So follow at your own risks ;p
// ==================================================================================
using StackExchange.Redis;

namespace Consumer
{
    class Program
    {
        private const string KEY_NAME = "Telemetry.Data";
        private const string GROUP_NAME = "Telemetry.Consumers";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");

            // Get database.
            var db = redis.GetDatabase();

            // Create consumer group if not exist.
            InitConsumerGroup(db);

            // Show consumers inside the consumer group.
            ShowConsumers(db);

            ConsoleKey? input = null;
            string? consumerName = string.Empty;

            do
            {
                if (string.IsNullOrWhiteSpace(consumerName))
                {
                    Console.Write("Enter New or Existing Consumer name: ");
                    consumerName = Console.ReadLine();
                    continue;
                }

                Console.Write("[Enter] to read stream, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();
                
                if (input != ConsoleKey.Enter) continue;

                ReadEntries(db, consumerName, ">");

            } while (input != ConsoleKey.X);

            // Delete the consumer.
            db.StreamDeleteConsumer(KEY_NAME, GROUP_NAME, consumerName);
        }

        private static void InitConsumerGroup(IDatabase db)
        {
            // Create consumer group if it does not exist.
            if (!db.KeyExists(KEY_NAME) || db.StreamGroupInfo(KEY_NAME).Length <= 0)
                db.StreamCreateConsumerGroup(KEY_NAME, GROUP_NAME, StreamPosition.NewMessages);
        }

        private static void ShowConsumers(IDatabase db)
        {
            // Get all the consumers in the consumer group.
            var consumers = db.StreamConsumerInfo(KEY_NAME, GROUP_NAME);
            if (consumers.Length <= 0) return;

            Console.WriteLine($"List of Consumers:");

            // Dump out the Consumer details.
            foreach (var c in consumers)
                Console.WriteLine($"  {c.Name} - Idle: {TimeSpan.FromMilliseconds(c.IdleTimeInMilliseconds).ToString()}.");

            Console.WriteLine();
        }

        private static void ReadEntries(IDatabase db, string? consumerName, string position, int count = 1)
        {
            // Read next item in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, GROUP_NAME, consumerName, position, count, true);

            if (entries.Length == 0)
            {
                Console.WriteLine("Stream is empty.");
                return;
            }

            // Read the entries.
            foreach (var entry in entries)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nStream Id: {entry.Id}");
                Console.ResetColor();

                // Read the values in the entry.
                foreach (var value in entry.Values)
                {
                    Console.WriteLine($"  {value.Name}: {((double)value.Value):0.00}");
                }
            }

            Console.WriteLine();
        }
    }
}
