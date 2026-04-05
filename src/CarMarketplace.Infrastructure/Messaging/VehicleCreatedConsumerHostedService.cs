using System.Text;
using System.Text.Json;
using CarMarketplace.Application.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarMarketplace.Infrastructure.Messaging;

/// <summary>Consumes VehicleCreated from RabbitMQ: logs and simulates email notification.</summary>
public class VehicleCreatedConsumerHostedService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<VehicleCreatedConsumerHostedService> _logger;
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public VehicleCreatedConsumerHostedService(
        Microsoft.Extensions.Options.IOptions<RabbitMqOptions> options,
        ILogger<VehicleCreatedConsumerHostedService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

        _channel.ExchangeDeclare(RabbitMqPublisher.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        var queueName = _channel.QueueDeclare("vehicle.created.notifications", durable: true, exclusive: false, autoDelete: false).QueueName;
        _channel.QueueBind(queue: queueName, exchange: RabbitMqPublisher.ExchangeName, routingKey: RabbitMqPublisher.VehicleCreatedRoutingKey);
        _channel.BasicQos(0, prefetchCount: 10, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<VehicleCreatedEvent>(json, JsonOptions);
                if (evt is null)
                {
                    _logger.LogWarning("VehicleCreated message could not be deserialized; acking.");
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation(
                    "VehicleCreated consumed: Id={VehicleId} {Brand} {Model} Price={Price}",
                    evt.VehicleId, evt.Brand, evt.Model, evt.Price);

                _logger.LogInformation(
                    "[Email simulation] To seller {SellerId}: Your listing for {Brand} {Model} is now live (VehicleId={VehicleId}).",
                    evt.SellerId, evt.Brand, evt.Model, evt.VehicleId);

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VehicleCreated; nack requeue.");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("VehicleCreated consumer started on queue {Queue}", queueName);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
