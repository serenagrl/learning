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
        private const string CHANNEL_NAME = "messages";

        static void Main(string[] args)
        {
            // Connects to the redis server.
            using var redis = ConnectionMultiplexer.Connect("localhost");
            var sub = redis.GetSubscriber(); // Get subscriber (used for pub/sub).

            do
            {
                Console.Write("Enter to broadcast messages, [Ctrl+C] to quit: ");
                Console.ReadLine(); 

                for (int i = 0; i < 5; i++)
                {
                    string message = $"Sample Message {i + 1}";
                    // Publish message to channel.
                    sub.Publish(CHANNEL_NAME, message);
                    Console.WriteLine($"Sent message {message}");
                }

            } while (true);
        }
    }
}
