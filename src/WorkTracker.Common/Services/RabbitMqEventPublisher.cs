using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WorkTracker.Common.Constants;
using WorkTracker.Common.Helpers;
using WorkTracker.Common.IntegrationEvents;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.Common.Services
{
    public sealed class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly IConnection _connection;

        public RabbitMqEventPublisher(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Publish Event to RabbitMQ exchange with given routing key. Routing key determines which queues receive the event based on their bindings.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cloudEvent"></param>
        /// <param name="routingKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PublishAsync<TData>(CloudEvent<TData> cloudEvent, string routingKey, CancellationToken cancellationToken = default)
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cloudEvent, JsonHelper.Options));

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = cloudEvent.Id,
                Type = cloudEvent.Type,
                Timestamp = new AmqpTimestamp(cloudEvent.Time.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMqKeys.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
