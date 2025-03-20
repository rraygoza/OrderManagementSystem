using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Shared.DTOs;
using ProductService.Services;
using System.Threading.Channels;

namespace ProductService.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _virtualHost;
        private readonly int _port;

        public RabbitMQConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<RabbitMQConsumer> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
            _userName = configuration["RabbitMQ:UserName"] ?? "guest";
            _password = configuration["RabbitMQ:Password"] ?? "guest";
            _virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
            _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");

            _logger.LogInformation($"RabbitMQConsumer connecting to: HostName={_hostName}, UserName={_userName}, VirtualHost={_virtualHost}, Port={_port}");
            try
            {
                var factory = new ConnectionFactory()
                {
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
                _channel.QueueDeclare(queue: "product_verification_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: "product_verification_response_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
                _logger.LogInformation("RabbitMQ connection and channel created successfully for consumer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RabbitMQ connection for consumer");
            }

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQConsumer.ExecuteAsync called.");
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                _logger.LogInformation("Received message from RabbitMQ.");
                try
                { 
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received message: {message}");

                    var request = JsonSerializer.Deserialize<ProductVerificationRequest>(message);

                    _logger.LogInformation($"Deserialized Request: ProductId={request?.ProductId}, Quantity={request?.Quantity}");


                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                        var response = productService.VerifyProductAsync(request).Result;
                        _logger.LogInformation($"VerifyProductAsync Result: IsValid={response.IsValid}, Price={response.Price}, Message={response.Message}");

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RabbitMQConsumer.Received");
                }
            };
            _channel.BasicConsume(queue: "product_verification_queue", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }

    }
}
