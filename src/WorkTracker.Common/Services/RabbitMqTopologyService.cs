using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using WorkTracker.Common.Constants;
using WorkTracker.Common.IntegrationEvents;

namespace WorkTracker.Common.Services
{
    /// <summary>
    /// Declares the exchange, queues, and bindings on startup.
    /// Publishers and consumers never need to redeclare topology.
    /// </summary>
    public sealed class RabbitMqTopologyService : IHostedService
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMqTopologyService> _logger;

        public RabbitMqTopologyService(IConnection connection, ILogger<RabbitMqTopologyService> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Declaring RabbitMQ topology on exchange '{Exchange}'", RabbitMqKeys.ExchangeName);

            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declare the single topic exchange all events flow through
            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqKeys.ExchangeName,
                type: RabbitMqKeys.ExchangeType,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // Declare every queue and bind it — add new events here
            await DeclareAndBindAsync(channel,
                queue: UserLoggedInEvent.QueueName,
                routingKey: UserLoggedInEvent.RoutingKey,    
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ topology declared successfully");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task DeclareAndBindAsync(
            IChannel channel,
            string queue,
            string routingKey,
            CancellationToken cancellationToken)
        {
            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: queue,
                exchange: RabbitMqKeys.ExchangeName,
                routingKey: routingKey,
                cancellationToken: cancellationToken);
        }
    }
}
