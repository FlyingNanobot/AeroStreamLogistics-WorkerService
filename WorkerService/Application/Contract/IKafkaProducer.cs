using System.Threading.Tasks;

namespace WorkerService.Application.Contract
{
    public interface IKafkaProducer
    {
        Task PublishAsync(string topic, object message);
    }
}
