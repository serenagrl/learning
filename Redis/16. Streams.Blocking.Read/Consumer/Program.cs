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
        private const string CHANNEL_NAME = "Telemetry.Channel";

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
            Console.WriteLine($"  No.\tStream-Id\t  Temperature\tHumidity");
            Console.WriteLine("  ---\t---------------\t  -----------\t--------");

            // Notes: Redis pub/sub Subscriber will not receive any messages if the they were published before
            // the Subscriber is online.  
            sub.Subscribe(CHANNEL_NAME, (channel, msg) =>
            {
                // Read next item in the stream.
                if (ReadEntries(db, consumerName, ">", 1))
                {
                    // Publish a message to inform self (or other subscribers) to read again until queue is empty.
                    sub.Publish(CHANNEL_NAME, "in", CommandFlags.FireAndForget);
                }
            });

            // Publish a message to activate own subscription.
            sub.Publish(CHANNEL_NAME, "in");
            
            Console.ReadKey();
            
            DoHouseKeeping(db, consumerName);
        }

        private static void DoHouseKeeping(IDatabase db, string consumerName)
        {
            // Delete Consumer from Group.
            if (db.KeyExists(KEY_NAME))
            {
                db.StreamDeleteConsumer(KEY_NAME, GROUP_NAME, consumerName);
                Console.WriteLine($"Consumer '{consumerName}' deleted from Consumer Group!");
            }
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

        private static bool ReadEntries(IDatabase db, string? consumerName, string position, int count = 1)
        {
            // Read entries in the stream.
            var entries = db.StreamReadGroup(KEY_NAME, GROUP_NAME, consumerName, position, count, true);

            if (entries.Length <= 0) return false;

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
                    Console.Write($"{((double)entry.Values[0].Value),11:0.00}\t");
                    Console.Write($"{((double)entry.Values[1].Value),8:0.00}\t");
                    Console.WriteLine();
                }
            }

            return true;
        }
    }
}
