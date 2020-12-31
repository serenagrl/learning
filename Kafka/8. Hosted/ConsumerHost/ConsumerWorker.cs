// ==================================================================================
// Developed by Serena Yeoh - December 2020
// Disclaimer: 
//   I wrote this during my career break for my self-learning and some parts may not
//   be that accurate. So follow at your own risks ;p
// ==================================================================================
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsumerHost
{
    public class ConsumerWorker : BackgroundService
    {
        private readonly ILogger<ConsumerWorker> _logger;
        private IConfiguration _config;
        private IConsumer<string, string> _consumer;

        private string _topic;
        private string _broker;

        public ConsumerWorker(ILogger<ConsumerWorker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _broker = config.GetValue<string>("Kafka:Broker");
            _topic = config.GetValue<string>("Kafka:Topic");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig()
            {
                BootstrapServers = _broker,
                GroupId = $"{_topic}.consumers",
                ClientId = $"{_topic}.consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = false,
                EnableAutoOffsetStore = false,
                EnablePartitionEof = true,
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();

            _logger.LogInformation("ConsumerHost started. Ready to process incoming messages.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Unsubscribe();
            _consumer.Close();
            _consumer.Dispose();

            _logger.LogInformation("ConsumerHost stopped.");

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                if (result.IsPartitionEOF)
                {
                    _logger.LogInformation($"Reached the end of partition {result.Partition.Value}. Waiting for more messages...");
                    continue;
                }

                _logger.LogInformation(
                    $"Processing message at {result.Offset.Value} in Partition {result.Partition.Value}:" +
                    $"{result.Message.Timestamp.UtcDateTime} - {result.Message.Key} - {result.Message.Value} ");

                try
                {
                    // Simulate processing
                    var random = new Random();
                    int count = random.Next(1, 5);
                    for (int i = 0; i < count; i++)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }

                    // Simulate error
                    if (random.Next(1, 50) % 3 == 0)
                        throw new ApplicationException("Simulated exception");

                    _logger.LogInformation("Message processed successfully.");
                    _consumer.StoreOffset(result);

                }
                catch (ApplicationException)
                {
                    // Reset the position of the offset to the errorneous record.
                    // Warning: This is only good for intermittent infrastructure failures
                    //          i.e. network interruptions, database timeouts.
                    //          This will result in an endless loop if you are rejecting based
                    //          on some business rules.
                    _consumer.Seek(result.TopicPartitionOffset);
                    _logger.LogError("Error processing message. Message will be retried later.");
                }
            }
        }
    }
}
