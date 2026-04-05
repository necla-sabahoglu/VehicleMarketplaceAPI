using System.Text;
using System.Text.Json;
using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CarMarketplace.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    public const string ExchangeName = "vehicle.events";
    public const string VehicleCreatedRoutingKey = "vehicle.created";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqOptions _options;
    private readonly object _sync = new();
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqPublisher(Microsoft.Extensions.Options.IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private void EnsureChannel()
    {
        if (_channel?.IsOpen == true)
            return;

        _channel?.Dispose();
        _connection?.Dispose();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

        _logger.LogInformation("RabbitMQ publisher connected; exchange {Exchange}", ExchangeName);
    }

    public Task PublishVehicleCreatedAsync(VehicleCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            EnsureChannel();
            if (_channel is null)
                throw new InvalidOperationException("RabbitMQ channel is not available.");

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt, JsonOptions));
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: VehicleCreatedRoutingKey,
                basicProperties: props,
                body: body);
        }

        _logger.LogDebug("Published VehicleCreated for vehicle {VehicleId}", evt.VehicleId);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
