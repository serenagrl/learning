// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //Note: Example will pretend to have an autonumber generator service.
            Console.WriteLine("Number server started. Listening for client request... ");
            using var service = new NumberService();
            service.Start();

            Console.ReadKey();
        }
    }
}
