using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared.Interfaces;
using Shared.Dtos;

namespace WebApi.Services;

public sealed class RabbitMqSubService(
    IServiceProvider serviceProvider,
    ILogger<RabbitMqSubService> logger,
    IConnectionFactory connectionFactory) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<RabbitMqSubService> _logger = logger;
    private readonly IConnectionFactory _connectionFactory = connectionFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("events", ExchangeType.Fanout, cancellationToken: stoppingToken);

        var queueDeclare = await channel.QueueDeclareAsync(cancellationToken: stoppingToken);
        var queueName = queueDeclare.QueueName;

        await channel.QueueBindAsync(queueName, "events", string.Empty, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            try
            {
                var evt = JsonSerializer.Deserialize<CommEventPayload>(json);
                if (evt == null)
                {
                    _logger.LogWarning("Received invalid JSON payload from exchange 'events'.");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var commService = scope.ServiceProvider.GetRequiredService<ICommService>();

                var success = await commService.AddStatusToHistoryAsync(evt);

                if (success)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Processed event for CommunicationId {CommunicationId}", evt.CommunicationId);
                }
                else
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    _logger.LogWarning("Failed to update DB for CommunicationId {CommunicationId}", evt.CommunicationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException) { }
    }
}
