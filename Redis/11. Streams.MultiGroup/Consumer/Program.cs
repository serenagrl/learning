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

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            ConsoleKey? input = null;
            string? consumerName = string.Empty;
            string? groupName = string.Empty;

            do
            {
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    Console.Write("Enter Consumer Group name: ");
                    groupName = Console.ReadLine();

                    // Create consumer group if not exist.
                    if (!string.IsNullOrWhiteSpace(groupName))
                        InitConsumerGroup(db, groupName);
                    
                    continue;

                }

                if (string.IsNullOrWhiteSpace(consumerName))
                {
                    // Show consumers inside the consumer group.
                    ShowConsumers(db, groupName);

                    Console.Write("Enter New or Existing Consumer name: ");
                    consumerName = Console.ReadLine();
                    continue;
                }


                Console.Write("[Enter] to read stream, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();
                
                if (input != ConsoleKey.Enter) continue;

                ReadEntries(db, groupName, consumerName, ">");

            } while (input != ConsoleKey.X);

            // Delete the consumer.
            db.StreamDeleteConsumer(KEY_NAME, groupName, consumerName);
        }

        private static void InitConsumerGroup(IDatabase db, string? groupName)
        {
            // Create consumer group if it does not exist.
            if (!db.KeyExists(KEY_NAME) || 
                string.IsNullOrWhiteSpace(db.StreamGroupInfo(KEY_NAME).FirstOrDefault(g => g.Name == groupName).Name))
                db.StreamCreateConsumerGroup(KEY_NAME, groupName, StreamPosition.Beginning);
        }

        private static void ShowConsumers(IDatabase db, string? groupName)
        {
            var groupInfo = db.StreamGroupInfo(KEY_NAME);
            if (string.IsNullOrWhiteSpace(groupInfo.FirstOrDefault(g => g.Name == groupName).Name)) return;

            // Get all the consumers that in the consumer group.
            var consumers = db.StreamConsumerInfo(KEY_NAME, groupName);
            if (consumers.Length <= 0) return;

            Console.WriteLine($"List of Consumers in '{groupName}':");

            // Dump out the Consumer details.
            foreach (var c in consumers)
                Console.WriteLine($"  {c.Name} - Idle: {TimeSpan.FromMilliseconds(c.IdleTimeInMilliseconds).ToString()}.");

            Console.WriteLine();
        }

        private static void ReadEntries(IDatabase db, string? groupName, string? consumerName, string position, int count = 1)
        {
            // Read next item in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, groupName, consumerName, position, count, true);

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
