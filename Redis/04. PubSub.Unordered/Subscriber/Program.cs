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
        private const string CHANNEL_NAME = "messages";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var sub = redis.GetSubscriber(); // Get subscriber (used for pub/sub).

            // Subscribe to channel concurrently. Messages may arrived in out-of-order.
            sub.Subscribe(CHANNEL_NAME, (channel, msg) => 
            { 
                Console.WriteLine((string)msg);
            });

            Console.WriteLine("Listening for messages ... (Press any key to exit).");
            Console.ReadKey();
        }
    }
}
