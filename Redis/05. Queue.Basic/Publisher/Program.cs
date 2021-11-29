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
        private const string KEY_NAME = "queue";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            string? message;

            do
            {
                Console.Write("Enter a message to send, [Ctrl+C] to exit: ");
                message = Console.ReadLine();

                // Push data into queue.
                db.ListLeftPush(KEY_NAME, message);

            } while (true);
        }
    }
}
