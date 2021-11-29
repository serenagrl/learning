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
        private const string CHANNEL_NAME = "Telemetry.Channel";
        private const string RECOVERY_CHANNEL_NAME = "Telemetry.Recovery";
        private const int MAX_DELIVERY = 6; // formula: retries x 2
        
        private static int _seq = 0;
        private static object _lockObj = new object();

        static void Main(string[] args)
        {
            Console.WriteLine("Message Consumer\n");

            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var sub = redis.GetSubscriber(); // Get subscriber (for pub/sub)
            var db = redis.GetDatabase();    // Get database.

            // Create consumer group if not exist.
            InitConsumerGroup(db);

            // Ask user to enter a name for the Consumer.
            string? consumerName = GetConsumerName(db);

            Console.WriteLine("Listening to messages... (any key to exit)\n");
            Console.WriteLine($"  No.\tStream-Id\t  Temperature\tHumidity   Retries   Acknowledge");
            Console.WriteLine("  ---\t---------------\t  -----------\t--------   -------   -----------");

            // Subscribe to the reovery channel to process pending messages.
            SubscribeToRecoveryChannel(sub, db, consumerName);

            // Subscribe to the main channel for processing messages.
            SubscribeToMainChannel(sub, db, consumerName);

            // Publish a message to activate own subscription.
            sub.Publish(CHANNEL_NAME, "in");
            
            Console.ReadKey();

            DoHouseKeeping(db, consumerName);
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

        private static string GetConsumerName(IDatabase db)
        {
            string? consumerName;
            do
            {
                // Show consumers inside the consumer group.
                ShowConsumers(db);

                Console.Write("Enter New or Existing Consumer name: ");
                consumerName = Console.ReadLine();

            } while (string.IsNullOrWhiteSpace(consumerName));
            
            return consumerName;
        }

        private static void SubscribeToRecoveryChannel(ISubscriber sub, IDatabase db, string consumerName)
        {
            // Subscribe to recovery channel for claiming pending messages.
            sub.Subscribe(RECOVERY_CHANNEL_NAME, (channel, msg) =>
            {
                // Get id from queue.
                var id = db.ListRightPop(QUEUE_NAME);

                if (!id.IsNull)
                {
                    // Claim pending message. This also increases the delivery count by 1.
                    db.StreamClaim(KEY_NAME, GROUP_NAME, consumerName, 0, new[] { id });

                    // Read pending messages (indicated by position 0). 
                    ReadEntries(sub, db, consumerName, "0");
                }
            });
        }

        private static void SubscribeToMainChannel(ISubscriber sub, IDatabase db, string consumerName)
        {
            // Subscribe to main channel for processing messages
            sub.Subscribe(CHANNEL_NAME, (channel, msg) =>
            {
                // Read next item in the stream.
                if (ReadEntries(sub, db, consumerName, ">", 1))
                {
                    // Publish a message to inform self (or other subscribers) to read again until queue is empty.
                    sub.Publish(CHANNEL_NAME, "in", CommandFlags.FireAndForget);
                }
            });
        }

        private static void DoHouseKeeping(IDatabase db, string consumerName)
        {
            if (db.KeyExists(KEY_NAME))
            {
                // Delete Consumer from Group.
                db.StreamDeleteConsumer(KEY_NAME, GROUP_NAME, consumerName);
                Console.WriteLine($"Consumer '{consumerName}' deleted from Consumer Group!");
            }
        }

        private static bool ReadEntries(ISubscriber sub, IDatabase db, string? consumerName,
            string position, int? count = null)
        {
            // Read entries in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, GROUP_NAME, consumerName, position, count);

            if (entries.Length == 0) return false;

            // Get pending messages.
            StreamPendingMessageInfo[]? pendings = null;
            if (position == "0")
                pendings = db.StreamPendingMessages(KEY_NAME, GROUP_NAME, entries.Length, consumerName);

            lock (_lockObj)
            {
                // Read the entries.
                foreach (var entry in entries)
                {
                    Console.Write($"  {++_seq,2}.\t");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{entry.Id}\t  ");
                    Console.ResetColor();

                    // Read the values in the entry.
                    Console.Write($"{((double)entry["Temperature"]),11:0.00}\t");
                    Console.Write($"{((double)entry["Humidity"]),8:0.00}  ");
                
                    // Show number of retries. Formula = Floor(DeliveryCount / 2)
                    Console.Write($"{(pendings?.Length > 0 ? Math.Floor(pendings[0].DeliveryCount / 2d) : 0), 8}   ");

                    // Randomly simulate error.
                    var random = new Random();
                    if (random.Next(1, 50) % 5 > 0)
                    {
                        // Poison message detection.
                        if (position == "0" && pendings?.Length > 0 &&
                            pendings?.First(p => p.MessageId == entry.Id).DeliveryCount >= MAX_DELIVERY)
                        {
                            // ACK the message first before removing it to maintain consistency.
                            db.StreamAcknowledge(KEY_NAME, GROUP_NAME, entry.Id);

                            // Delete the message from the stream. (or you may log it somewhere).
                            db.StreamDelete(KEY_NAME, new[] { entry.Id });

                            Console.WriteLine("Poison Discarded ");
                        }
                        else
                        {
                            // Push the id into queue.
                            db.ListLeftPush(QUEUE_NAME, entry.Id);

                            Console.WriteLine("Error ");

                            // Publish a notification to other Consumers.
                            sub.Publish(RECOVERY_CHANNEL_NAME, entry.Id);
                        }
                    }
                    else
                    {
                        // Acknowledge the message so that it won't be read by any Consumers.
                        db.StreamAcknowledge(KEY_NAME, GROUP_NAME, entry.Id);
                        Console.WriteLine("Yes");
                    }
                }   
            }

            return true;
        }

    }
}
