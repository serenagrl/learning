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

            // Get channel.
            var channel = sub.Subscribe(CHANNEL_NAME);

            // Handle messages sequentially.
            channel.OnMessage(msg =>
            {
                Console.WriteLine(msg.Message);
            });

            Console.WriteLine("Listening for messages ... (Press any key to exit).");
            Console.ReadKey();
        }
    }

}
