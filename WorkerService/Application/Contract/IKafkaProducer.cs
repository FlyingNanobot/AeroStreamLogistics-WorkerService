using System.Threading.Tasks;

namespace WorkerService.Application.Contract
{
    /// <summary>
    /// Abstraction for publishing messages to Kafka topics.
    /// </summary>
    /// <remarks>
    /// Implementations should be registered as singletons and handle their own producer
    /// lifecycle and fault logging. Messages are serialized to JSON by default.
    /// </remarks>
    public interface IKafkaProducer
    {
        /// <summary>
        /// Publish an object message to the specified topic asynchronously.
        /// </summary>
        /// <param name="topic">Kafka topic name.</param>
        /// <param name="message">Object to serialize and publish.</param>
        Task PublishAsync(string topic, object message);
    }
}
