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
        private const string KEY_NAME = "queue";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var db = redis.GetDatabase(); // Get database.

            string message = string.Empty;

            do
            {
                Console.Write("[Enter] to read queue, [Ctrl+C] to exit: ");
                Console.ReadLine();

                // Read from Queue.
                // Note: StackExchange.Redis does not explose the blocking Pop function when reading queues.
                //       Client will exit and return no messages when queue is empty.
                message = db.ListRightPop(KEY_NAME);

                if (!string.IsNullOrWhiteSpace(message))
                    Console.WriteLine($"Message received from queue: {message}");
                else
                    Console.WriteLine("Queue is empty. Please push some message into the queue.");

            } while (true);

        }
    }
}
