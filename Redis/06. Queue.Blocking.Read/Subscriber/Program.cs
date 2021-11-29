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
        private const string CHANNEL_NAME = "queueManager";

        static void Main(string[] args)
        {
            Console.WriteLine("Listening for messages ... (Press any key to exit).");
            
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var sub = redis.GetSubscriber(); // Get subscriber (for pub/sub)
            var db = redis.GetDatabase();    // Get database.

            // Notes: Redis pub/sub Subscriber will not receive any messages if the they were published before
            // the Subscriber is online. The StackExchange.Redis library does not provide blocking pops (BRPOP)
            // for reading queues due to protecting the multiplexer. Therefore, the following workaround is 
            // needed when using Redis to read queues.
            sub.Subscribe(CHANNEL_NAME, (channel, msg) =>
            {
                // Read from Queue.
                string message = db.ListRightPop(KEY_NAME);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine($"Message received from queue: {message}");
                    
                    // Publish a message to inform self (or other subscribers) to read again until queue is empty.
                    sub.Publish(CHANNEL_NAME, "in", CommandFlags.FireAndForget);
                }
            });

            // Publish a message to activate own subscription.
            // Note: This will trigger subscriber to read any messages that are in the queue.
            sub.Publish(CHANNEL_NAME, "in");
            Console.ReadKey();
        }
    }
}
