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
        private const string CHANNEL_NAME = "queueManager";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var sub = redis.GetSubscriber(); // Get subscriber (for pub/sub)
            var db = redis.GetDatabase();    // Get database.

            string? message;

            // Notes: Redis pub/sub Subscriber will not receive any messages if the they were published before
            // the Subscriber is online. The StackExchange.Redis library does not provide blocking pops (BRPOP)
            // for reading queues due to protecting the multiplexer. Therefore, the following workaround is 
            // needed when using Redis to read queues.
            do
            {
                Console.Write("Enter message to send, [Ctrl+C] to quit: ");
                message = Console.ReadLine();

                // Push data into queue.
                db.ListLeftPush(KEY_NAME, message);

                // Notify subsribers. 
                // Note: Using this way, data that is required for processing will not be lost because it is
                // inside the queue and will be ok if no subscribers were listening at this point in time.
                sub.Publish(CHANNEL_NAME, "in");

            } while (true);
        }
    }
}
