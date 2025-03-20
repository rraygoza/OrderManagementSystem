using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace OrderService.Messaging
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQProducer> _logger;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _virtualHost;
        private readonly int _port;

        public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
        {
            _logger = logger;
            _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
            _userName = configuration["RabbitMQ:UserName"] ?? "rabbituse";
            _password = configuration["RabbitMQ:Password"] ?? "rabbitpassword";
            _virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
            _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
            _logger.LogInformation($"RabbitMQProducer connecting to: HostName={_hostName}, UserName={_userName}, VirtualHost={_virtualHost}, Port={_port}");
            try
            {
                var factory = new ConnectionFactory() {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password,
                    VirtualHost = _virtualHost,
                    Port = _port,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("RabbitMQ connection and channel created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RabbitMQ connection");
            }
        }
        public IModel Channel => _channel;

        public void PublishMessage<T>(string queueName, T message, string correlationId)
        {
            _logger.LogInformation($"Attempting to publish message to queue: {queueName}");
            if (_channel == null || _connection == null || !_connection.IsOpen)
            {
                _logger.LogError("RabbitMQ connection is not open.  Cannot publish message.");
                throw new InvalidOperationException("RabbitMQ connection is not open.");
            }
            try
            {
                if (Encoding.UTF8.GetByteCount(queueName) > 255)
                {
                    throw new Exception("Queue name is too long");
                }
                _channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var properties = _channel.CreateBasicProperties();
                properties.CorrelationId = correlationId;
                properties.ReplyTo = queueName;
                _channel.BasicPublish(exchange: "",
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ");
                throw;
            }
        }
    }
}
