// ==================================================================================
// Developed by Serena Yeoh - November 2021
// Disclaimer: 
//   I wrote this for my self-learning and some parts may not be that accurate.
//   So follow at your own risks ;p
// ==================================================================================
using StackExchange.Redis;

namespace Publisher
{
    class Program
    {
        private const string KEY_NAME = "Telemetry.Data";

        static void Main(string[] args)
        {
            Console.WriteLine("Message Publisher\n");

            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            ConsoleKey input;

            do
            {
                Console.Write("[Enter] to send message, [I] Information [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();

                if (input == ConsoleKey.I) ShowInfo(db);
                if (input != ConsoleKey.Enter) continue;
                
                SendMessage(db);

            } while (input != ConsoleKey.X);

            // House-keeping - removes the stream.
            Console.WriteLine("Housekeeping - Deleting stream.");
            db.KeyDelete(KEY_NAME);

        }

        private static void SendMessage(IDatabase db)
        {
            var rand = new Random();

            // Randomize some telemetry data
            double temperature = rand.Next(10, 50) + rand.NextDouble();
            double humidity = rand.Next(50, 90) + rand.NextDouble();

            // Create entries.
            var values = new NameValueEntry[]
            {
                new NameValueEntry("Temperature", temperature),
                new NameValueEntry("Humidity", humidity),
            };

            // Push to stream.
            var id = db.StreamAdd(KEY_NAME, values);

            Console.WriteLine($"\nSent Temperature: {temperature:0.00}, Humidity: {humidity:0.00}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Returned Id:{id}\n");
            Console.ResetColor();
        }

        private static void ShowInfo(IDatabase db)
        {
            // Get Stream info.
            var streamInfo = db.StreamInfo(KEY_NAME);
            Console.WriteLine($"\nStream Name: {KEY_NAME} ");
            Console.WriteLine($"Total Messages: {streamInfo.Length} ");
            Console.WriteLine($"Last Generated Id: {streamInfo.LastGeneratedId} ");
            Console.WriteLine($"Total Consumer Groups: {streamInfo.ConsumerGroupCount} ");

            // Get Consumer Groups info.
            var groupInfo = db.StreamGroupInfo(KEY_NAME);
            foreach (var gi in groupInfo)
            {
                Console.WriteLine($"\nConsumer Group Name: {gi.Name} ");
                Console.WriteLine($"Total Consumers: {gi.ConsumerCount} ");
                Console.WriteLine($"Total Pending Messages: {gi.PendingMessageCount} ");

                // Get all the consumers in the consumer group.
                var consumers = db.StreamConsumerInfo(KEY_NAME, gi.Name);
                if (consumers.Length > 0)
                {
                    Console.WriteLine($"List of Consumers:");

                    // Dump out the Consumer names.
                    foreach (var c in consumers)
                        Console.WriteLine($"  {c.Name} - {c.PendingMessageCount} pending messages, Idle: {TimeSpan.FromMilliseconds(c.IdleTimeInMilliseconds).ToString()}.");

                    Console.WriteLine();
                }

                Console.WriteLine($"Last Delivered Id: {gi.LastDeliveredId} ");

                // Get list of unread entries.
                var entries = db.StreamRange(KEY_NAME, $"({gi.LastDeliveredId}", "+");
                Console.WriteLine($"Total unread messages: {entries.Length}");

                if (entries.Length <= 0) continue;

                Console.WriteLine($"\n  No.\tStream-Id\t  Temperature\tHumidity");
                Console.WriteLine("  ---\t---------------\t  -----------\t--------");

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
                    Console.Write($"{((double)entry["Humidity"]),8:0.00}  ");

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }
}
