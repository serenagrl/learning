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

                SendMessage(db);

            } while (input != ConsoleKey.X);

            // House-keeping - removes the stream.
            Console.WriteLine("Housekeeping - Deleting stream.");
            db.KeyDelete(KEY_NAME);

        }

        private static void SendMessage(IDatabase db)
        {
            var rand = new Random();

            // Create entries.
            var values = new NameValueEntry[]
            {
                // Randomize some telemetry data
                new NameValueEntry("Temperature", rand.Next(10, 50) + rand.NextDouble()),
                new NameValueEntry("Humidity", rand.Next(50, 90) + rand.NextDouble()),
            };

            // Push to stream.
            var id = db.StreamAdd(KEY_NAME, values);

            Console.WriteLine($"\nSent Temperature: {double.Parse(values[0].Value):0.00}, Humidity: {double.Parse(values[1].Value):0.00}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Returned Id:{id}\n");
            Console.ResetColor();
        }
    }
}
