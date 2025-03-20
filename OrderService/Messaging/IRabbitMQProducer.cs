using RabbitMQ.Client;
using System.Threading.Channels;

namespace OrderService.Messaging
{
    public interface IRabbitMQProducer
    {
        IModel Channel { get; }
        void PublishMessage<T>(string queueName, T message, string correlationId);
    }
}
