namespace ProductService.Messaging
{
    public interface IRabbitMQProducer
    {
        void PublishMessage<T>(string queueName, T message);
    }
}