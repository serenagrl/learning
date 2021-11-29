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
        private const string HOME_KEY_NAME_1 = "Telemetry.Home1";
        private const string HOME_KEY_NAME_2 = "Telemetry.Home2";

        // Note: All Consumers will get the same messages from the stream. This is Fanout.
        // Launch another instance of this to see the results.
        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            // Start from the begining of the stream.
            var ids = new Dictionary<string, string>()
            {
                {  HOME_KEY_NAME_1, "0-0" },
                {  HOME_KEY_NAME_2, "0-0" }
            };

            ConsoleKey input;

            do
            {
                Console.Write("[Enter] to read stream, [X] to exit: ");
                input = Console.ReadKey().Key;
                Console.WriteLine();
                
                if (input != ConsoleKey.Enter) continue;

                ReadEntriesAndUpdateIds(db, ids);

            } while (input != ConsoleKey.X);
        
        }

        private static void ReadEntriesAndUpdateIds(IDatabase db, Dictionary<string, string> positions)
        {
            // Read entries in the stream.
            var streams = db.StreamRead(new StreamPosition[]
            {
                new StreamPosition(HOME_KEY_NAME_1, positions[HOME_KEY_NAME_1]),
                new StreamPosition(HOME_KEY_NAME_2, positions[HOME_KEY_NAME_2])
            });

            if (streams.Length == 0)
            {
                Console.WriteLine("No new messages.");
                return;
            }

            Console.WriteLine($"\n  No.\tStream Name\t\tStream-Id\t  Temperature\tHumidity");
            Console.WriteLine("  ---\t-------------\t\t---------------\t  -----------\t--------");

            int seq = 0;

            foreach (var stream in streams)
            {
                // Read the entries.
                foreach (var entry in stream.Entries)
                {
                    Console.Write($"  {++seq, 2}.\t");
                    Console.Write($"{stream.Key}\t\t");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{entry.Id}\t  ");
                    Console.ResetColor();

                    // Read the values in the entry.
                    Console.Write($"{((double)entry["Temperature"]),11:0.00}\t");
                    Console.Write($"{((double)entry["Humidity"]),8:0.00}\t");
                    Console.WriteLine();

                    // Remember the previous id.
                    positions[stream.Key] = entry.Id;
                }
            }

            Console.WriteLine();

            return;
        }

    }
}
