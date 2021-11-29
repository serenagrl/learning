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
        private const string HOME_KEY_NAME_1 = "Telemetry.Home1";
        private const string HOME_KEY_NAME_2 = "Telemetry.Home2";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            ConsoleKey input;

            do
            {
                Console.Write("[Enter] to send message, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();

                if (input != ConsoleKey.Enter) continue;

                Console.WriteLine($"\n  Stream Name\t\tStream-Id\t  Temperature\tHumidity");
                Console.WriteLine("  -------------\t\t---------------\t  -----------\t--------");

                // Send messages.
                SendMessage(db, HOME_KEY_NAME_1);
                SendMessage(db, HOME_KEY_NAME_2);
                
                Console.WriteLine();

            } while (input != ConsoleKey.X);

            // House-keeping - removes the stream.
            Console.WriteLine("Housekeeping - Deleting stream.");
            db.KeyDelete(HOME_KEY_NAME_1);
            db.KeyDelete(HOME_KEY_NAME_2);

        }

        private static void SendMessage(IDatabase db, string streamName)
        {
            var rand = new Random();

            // Create entries.
            var values = new NameValueEntry[]
            {
                // Randomize some telemetry data
                new NameValueEntry("Temperature", rand.Next(10, 50) + rand.NextDouble()),
                new NameValueEntry("Humidity", rand.Next(50, 90) + rand.NextDouble()),
            };

            var id = db.StreamAdd(streamName, values);

            Console.Write($"  {streamName}\t");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{id}\t  ");
            Console.ResetColor();

            // Read the values in the entry.
            Console.Write($"{((double)values[0].Value),11:0.00}\t");
            Console.Write($"{((double)values[0].Value),8:0.00}\t");
            Console.WriteLine();
        }
    }
}
