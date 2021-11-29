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
                        
            // Default to beginning of stream.
            string id = "0-0";

            // Get last id of messages.
            // Note: This makes it only read new messages.
            var last = db.StreamRange(KEY_NAME, "-", "+", 1, Order.Descending);
            if (last.Length> 0) 
                id = last[0].Id;

            ConsoleKey input;

            do
            {
                Console.Write("[Enter] to read stream, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();
                
                if (input != ConsoleKey.Enter) continue;

                id = ReadEntries(db, id);
                
            } while (input != ConsoleKey.X);
        
        }

        private static string ReadEntries(IDatabase db, string position)
        {
            // Read entries in the stream.
            var entries = db.StreamRead(KEY_NAME, position);

            if (entries.Length == 0)
            {
                Console.WriteLine("No new messages.");
                return position;
            }

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
                Console.Write($"{((double)entry["Humidity"]),8:0.00}\t");
                Console.WriteLine();

                position = entry.Id;
            }

            Console.WriteLine();

            return position;
        }

    }
}
