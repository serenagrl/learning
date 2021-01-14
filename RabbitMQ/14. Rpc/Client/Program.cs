// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using System;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using var agent = new NumberAgent();

            while (true)
            {
                // Get how many times request or quit.
                if (!GetNoOfTimesToRequest(out int times)) break;

                for (int i = 1; i <= times; i++)
                {
                    Console.Write($"Received number: {agent.GetNumber()}. Pretending to do some process ");
                    
                    // Simulate work
                    var random = new Random();
                    int count = random.Next(1, 3);
                    for (int j = 0; j < count; j++)
                    {
                        Console.Write(".");
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine($" done.");
                }

                Console.WriteLine($"{times} numbers requested.");
            }

            Console.ReadKey();
        }

        private static bool GetNoOfTimesToRequest(out int times)
        {
            Console.Write("Enter how many numbers to request or [Q] to quit: ");

            string input = Console.ReadLine();
            bool quit = (input.ToUpper() == "Q");

            times = 0;
            if (!quit && !int.TryParse(input, out times))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid number of messages.");
                Console.ResetColor();
            }

            return !quit;
        }
    }
}