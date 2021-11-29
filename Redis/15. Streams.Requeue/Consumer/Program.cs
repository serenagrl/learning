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
        private const string QUEUE_NAME = "Telemetry.Pending";
        private const string CHANNEL_NAME = "Telemetry.Recovery";

        static void Main(string[] args)
        {
            Console.WriteLine("Message Consumer\n");

            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var sub = redis.GetSubscriber(); // Get subscriber (for pub/sub)
            var db = redis.GetDatabase();    // Get database.

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

                    if (!string.IsNullOrWhiteSpace(consumerName))
                    {
                        // Subscribe to recovery channel.
                        sub.Subscribe(CHANNEL_NAME, (channel, msg) =>
                        {
                            // Get id from queue.
                            var id = db.ListRightPop(QUEUE_NAME);

                            // Claim the pending message.
                            if (!id.IsNull)
                                db.StreamClaim(KEY_NAME, GROUP_NAME, consumerName, 0, new[] { id });
                        });
                    }

                    continue;
                }

                Console.Write($"{consumerName}$ [Enter] to read stream, [P]ending messages, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();

                // Reading from position 0 will return all pending messages for the Consumer.
                if (input == ConsoleKey.P) ReadEntries(sub, db, consumerName, "0");
                
                if (input != ConsoleKey.Enter) continue;

                // Reading from position '>' will read next new item in the stream.
                ReadEntries(sub, db, consumerName, ">", 1);

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

        private static void ReadEntries(ISubscriber sub, IDatabase db, string? consumerName,
            string position, int? count = null)
        {
            // Read entries in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, GROUP_NAME, consumerName, position, count);

            if (entries.Length == 0)
            {
                Console.WriteLine("No messages.");
                return;
            }

            // Get pending messages.
            StreamPendingMessageInfo[]? pendings = null;
            if (position == "0")
                pendings = db.StreamPendingMessages(KEY_NAME, GROUP_NAME, entries.Length, consumerName);

            Console.WriteLine($"\n  Processing {entries.Length} message(s)\n");
            Console.WriteLine($"  No.\tStream-Id\t  Temperature\tHumidity\tAcknowledge [y/N]");
            Console.WriteLine("  ---\t---------------\t  -----------\t--------\t-----------------");

            int seq = 0;

            // Read the entries.
            foreach (var entry in entries)
            {
                Console.Write($"  {++seq,2}.\t");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{entry.Id}\t  ");
                Console.ResetColor();

                // Read the values in the entry.
                Console.Write($"{((double)entry["Temperature"]),11:0.00}\t");
                Console.Write($"{((double)entry["Humidity"]),8:0.00}\t");

                int pos = Console.CursorLeft;
                Console.Write("> ");
                ConsoleKey input = Console.ReadKey().Key;
                Console.CursorLeft = pos;

                if (input == ConsoleKey.Y)
                {
                    // Acknowledge the message so that it won't be read by any Consumers.
                    db.StreamAcknowledge(KEY_NAME, GROUP_NAME, entry.Id);
                    Console.WriteLine("Yes");
                }
                else
                {
                    // Check for poison message.
                    if (position == "0" && pendings?.Length > 0 &&
                        pendings?.First(p => p.MessageId == entry.Id).DeliveryCount >= 6)
                    {
                        // ACK the message to maintain consistency.
                        db.StreamAcknowledge(KEY_NAME, GROUP_NAME, entry.Id);

                        // Remove the message from the stream.
                        db.StreamDelete(KEY_NAME, new[] { entry.Id });
                        Console.WriteLine("Poison Discarded ");
                    }
                    else
                    {
                        // Push data into queue.
                        db.ListLeftPush(QUEUE_NAME, entry.Id);
                        Console.WriteLine("No ");

                        // Notify Consumers to process it.
                        sub.Publish(CHANNEL_NAME, entry.Id);
                    }
                }
            }

            Console.WriteLine();
        }

    }
}
