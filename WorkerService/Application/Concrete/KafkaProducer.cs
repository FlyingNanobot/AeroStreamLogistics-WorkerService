using Confluent.Kafka;
using System.Text.Json;
using WorkerService.Application.Contract;

namespace WorkerService.Application.Concrete
{
    /// <summary>
    /// Kafka producer implementation using Confluent.Kafka. Serializes messages as JSON.
    /// </summary>
    /// <remarks>
    /// The producer is configured for idempotent delivery and basic performance tuning.
    /// This class manages the producer lifecycle and should be registered as a singleton.
    /// </remarks>
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IConfiguration config)
        {
            /// <summary>
            /// Create and configure a Kafka producer from IConfiguration.
            /// </summary>
            /// <remarks>
            /// The constructor reads BootstrapServers and applies sensible defaults for
            /// idempotence and latency/throughput trade-offs.
            /// </remarks>
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
            /// <summary>
            /// Serialize the provided message to JSON and publish to the given topic.
            /// </summary>
            /// <remarks>
            /// The method catches ProduceException to avoid bubbling transport errors
            /// to callers; adapt error handling if you need retry semantics.
            /// </remarks>
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
            /// <summary>
            /// Dispose the underlying producer (flushes pending messages first).
            /// </summary>
            _producer.Flush();
            _producer.Dispose();
        }
    }
}