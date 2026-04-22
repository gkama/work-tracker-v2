using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WorkTracker.Common.IntegrationEvents;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.Common.Services
{
    public sealed class RabbitMqEventPublisher : IEventPublisher
    {
        private const string ExchangeName = "worktracker.events";
        private const string ExchangeType = "topic";

        private readonly IConnection _connection;

        public RabbitMqEventPublisher(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task PublishAsync<TData>(CloudEvent<TData> cloudEvent, string routingKey, CancellationToken cancellationToken = default)
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cloudEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }));

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = cloudEvent.Id,
                Type = cloudEvent.Type,
                Timestamp = new AmqpTimestamp(cloudEvent.Time.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
