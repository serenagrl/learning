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
        private const string KEY_NAME = "Telemetry.Small";
        private const string GROUP_NAME = "Telemetry.Consumers";

        static void Main(string[] args)
        {
            Console.WriteLine("Message Consumer\n");

            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            // Create consumer group if not exist.
            InitConsumerGroup(db);

            ConsoleKey? input = null;
            string? consumerName = string.Empty;

            do
            {
                if (string.IsNullOrWhiteSpace(consumerName))
                {
                    ShowConsumers(db);

                    Console.Write("Enter Consumer name: ");
                    consumerName = Console.ReadLine();
                    continue;
                }

                Console.Write("[Enter] to read stream, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();

                if (input != ConsoleKey.Enter) continue;

                // Read next item in the stream.
                ReadEntries(db, consumerName, ">", 1);

            } while (input != ConsoleKey.X);

            // Delete Consumer from Group.
            if (db.KeyExists(KEY_NAME))
            {
                db.StreamDeleteConsumer(KEY_NAME, GROUP_NAME, consumerName);
                Console.WriteLine($"Consumer '{consumerName}' deleted from Consumer Group!");
            }
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
                Console.WriteLine($"  {c.Name} - {c.PendingMessageCount} pending messages, Idle: {TimeSpan.FromMilliseconds(c.IdleTimeInMilliseconds).ToString()}.");

            Console.WriteLine();
        }

        private static void ReadEntries(IDatabase db, string? consumerName, string position, int? count = null)
        {
            // Read entries in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, GROUP_NAME, consumerName, position, count);

            if (entries.Length == 0)
            {
                Console.WriteLine("No messages.");
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
                    Console.WriteLine($"  {value}");

                // Acknowledge the message so that it won't be read by any Consumers.
                db.StreamAcknowledge(KEY_NAME, GROUP_NAME, entry.Id);

                Console.WriteLine();
            }
        }

    }
}
