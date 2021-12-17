// ==================================================================================
// Developed by Serena Yeoh - January 2021
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Consumer
{
    class Program
    {
        private const string QUEUE_NAME = "learning.requeue";
        
        static void Main(string[] args)
        {
            Console.WriteLine("Launch two or more Consumer instances to test.");
            Console.WriteLine("Consumer started. Waiting to process messages...");

            // Default connection settings - localhost, guest/guest
            var factory = new ConnectionFactory();

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Note: Queue set to auto delete when exit. Change to true if want to keep it.
            channel.QueueDeclare(QUEUE_NAME, false, false, true);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received; // Cleaner than anonymous method.

            channel.BasicConsume(QUEUE_NAME, false, consumer);
            Console.ReadKey();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            byte[] body = e.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            Console.Write($"Processing {message} ");

            IModel channel = (sender as EventingBasicConsumer)?.Model;

            // Note: This kind of retries are only suitable for infrastructure or network
            //       connectivity issues. Need to handle it differently if validating 
            //       business logic/rules.  
            try
            {
                // Simulate processing/error.
                ProcessWithRandomError();

                Console.WriteLine(" done.");
                channel?.BasicAck(e.DeliveryTag, false);
            }
            catch(ApplicationException ex) when (ex.Message.Contains("Simulated exception"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" error!");
                Console.ResetColor();

                // Negative acknowledgement after error and set to requeue.
                // Note: If this is not done, the messages will be stucked in queue
                //       until this console is terminated.
                channel?.BasicNack(e.DeliveryTag, false, true);
            }

            // Check to see if there are anymore messages.
            // Note: May not be 100% accurate.
            if (channel?.MessageCount(QUEUE_NAME) <= 0)
                Console.WriteLine("Process completed. Waiting for more messages to process...");
        }

        #region Unrelated stuff
        private static void ProcessWithRandomError()
        {
            var random = new Random();
            int count = random.Next(1, 5);
            for (int i = 0; i < count; i++)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }

            // Simulate error
            if (random.Next(1, 50) % 3 == 0)
                throw new ApplicationException("Simulated exception");
        }
        #endregion
    }
}
