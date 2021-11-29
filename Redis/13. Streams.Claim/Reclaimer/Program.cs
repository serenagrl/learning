// ==================================================================================
// Developed by Serena Yeoh - November 2021
// Disclaimer: 
//   I wrote this for my self-learning and some parts may not be that accurate.
//   So follow at your own risks ;p
// ==================================================================================
using StackExchange.Redis;

namespace Subscriber
{
    class Program
    {
        private const string KEY_NAME = "Telemetry.Data";
        private const string GROUP_NAME = "Telemetry.Consumers";
        private const string CONSUMER_NAME = "Reclaimer";

        static void Main(string[] args)
        {
            Console.WriteLine("Message Reclaimer\n");

            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            // Create consumer group if not exist.
            InitConsumerGroup(db);

            do
            {
                // Show consumers inside the consumer group.
                bool hasConsumers = ShowConsumers(db);

                Console.Write("Claim pending messages from Consumer, [Enter] Refresh, [Ctrl+C] to quit: ");
                string? claimFrom = Console.ReadLine();
                Console.WriteLine();

                if (string.IsNullOrWhiteSpace(claimFrom)) continue;

                // Claim messages.
                if (hasConsumers) ClaimMessages(db, claimFrom);

            } while (true);
        }

        private static void ClaimMessages(IDatabase db, string claimFrom)
        {
            // Get pending messages for target Consumer.
            var pendingMessages = db.StreamPendingMessages(KEY_NAME, GROUP_NAME, 99, claimFrom, "0-0");
            if (pendingMessages.Length <= 0)
            {
                Console.WriteLine($"Consumer '{claimFrom}' has no pending messages.\n");
                return;
            }

            Console.WriteLine($"Claiming {pendingMessages.Length} messages from '{claimFrom}':\n");

            // Claim the pending messages.
            db.StreamClaim(KEY_NAME, GROUP_NAME, CONSUMER_NAME, 0, pendingMessages.Select(m => m.MessageId).ToArray());

            // Process the pending message.
            ProcessEntries(db, "0");

            // Delete ownself from the Consumer Group.
            db.StreamDeleteConsumer(KEY_NAME, GROUP_NAME, CONSUMER_NAME);
        }

        private static void InitConsumerGroup(IDatabase db)
        {
            // Create consumer group if it does not exist.
            if (!db.KeyExists(KEY_NAME) || db.StreamGroupInfo(KEY_NAME).Length <= 0)
                db.StreamCreateConsumerGroup(KEY_NAME, GROUP_NAME, StreamPosition.NewMessages);
        }

        private static bool ShowConsumers(IDatabase db)
        {
            // Get all the consumers in the consumer group.
            var consumers = db.StreamConsumerInfo(KEY_NAME, GROUP_NAME);
            if (consumers.Length <= 0) return false;

            Console.WriteLine("List of Consumers:");

            // Dump out the Consumer names.
            foreach (var c in consumers)
                Console.WriteLine($"  {c.Name} - {c.PendingMessageCount} pending messages, Idle: {TimeSpan.FromMilliseconds(c.IdleTimeInMilliseconds).ToString()}.");

            Console.WriteLine();

            return true;
        }

        private static void ProcessEntries(IDatabase db, string position)
        {
            // Read next item in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, GROUP_NAME, CONSUMER_NAME, position);

            if (entries.Length == 0)
            {
                Console.WriteLine("No messages.");
                return;
            }

            Console.WriteLine($"  No.\tStream-Id\t  Temperature\tHumidity\t[A]cknowledge [R]epublish [D]iscard");
            Console.WriteLine("  ---\t---------------\t  -----------\t--------\t-----------------------------------");

            int seq = 0;

            // Read the entries.
            foreach (var entry in entries)
            {
                Console.Write($"  {++seq,2}.\t");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{entry.Id}\t  ");
                Console.ResetColor();

                // Read the values in the entry.
                Console.Write($"{((double)entry["Temperature"]), 11:0.00}\t");
                Console.Write($"{((double)entry["Humidity"]), 8:0.00}\t");

                int pos = Console.CursorLeft;
                Console.Write("> ");
                ConsoleKey input = Console.ReadKey().Key;
                Console.CursorLeft = pos;

                switch (input)
                {
                    case ConsoleKey.A:
                        // ACK the message.
                        db.StreamAcknowledge(KEY_NAME, GROUP_NAME, entry.Id);
                        Console.WriteLine($"Acknowledged");
                        break;

                    case ConsoleKey.R:
                        // Republish the message. Message will get a new Id.
                        var id = db.StreamAdd(KEY_NAME, entry.Values);
                        Console.WriteLine($"Republished (Id: {id})");

                        // Delete the old message.
                        // Note: You can ACK the message as an alternative but that will leave the old entry
                        //       inside the stream.
                        db.StreamDelete(KEY_NAME, new[] { entry.Id });
                        break;

                    default:
                        // Delete the message.
                        // Note: You can also push it to a queue serving as a poison queue if you wish.
                        db.StreamDelete(KEY_NAME, new[] { entry.Id });
                        Console.WriteLine($"Discarded");
                        break;
                }
            }

            Console.WriteLine();
        }

    }
}