using Confluent.Kafka;
using System.Text.Json;
using WorkerService.Application.Contract;

namespace WorkerService.Application.Concrete
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IConfiguration config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],

                // Required when idempotence is enabled
                Acks = Acks.All,
                EnableIdempotence = true,

                // Performance tuning
                LingerMs = 5,
                BatchSize = 32 * 1024,
                CompressionType = CompressionType.Snappy,
                MessageTimeoutMs = 3000
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) =>
                {
                    Console.WriteLine($"[Kafka] Error: {e.Reason}");
                })
                .Build();
        }

        public async Task PublishAsync(string topic, object message)
        {
            var json = JsonSerializer.Serialize(message);

            try
            {
                await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = json
                });
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"[Kafka] Delivery failed: {ex.Error.Reason}");
            }
        }

        public void Dispose()
        {
            _producer.Flush();
            _producer.Dispose();
        }
    }
}