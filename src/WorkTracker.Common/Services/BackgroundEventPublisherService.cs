using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using WorkTracker.Common.IntegrationEvents;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.Common.Services
{
    public sealed class BackgroundEventPublisherService : BackgroundService, IBackgroundEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<BackgroundEventPublisherService> _logger;
        private readonly Channel<QueuedEvent> _queue;

        public BackgroundEventPublisherService(
            IEventPublisher eventPublisher,
            ILogger<BackgroundEventPublisherService> logger)
        {
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = Channel.CreateUnbounded<QueuedEvent>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cloudEvent"></param>
        /// <param name="routingKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task QueueAsync<TData>(CloudEvent<TData> cloudEvent, string routingKey, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(cloudEvent);
            ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);

            var queuedEvent = new QueuedEvent(
                routingKey,
                ct => _eventPublisher.PublishAsync(cloudEvent, routingKey, ct));

            await _queue.Writer.WriteAsync(queuedEvent, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var queuedEvent in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await queuedEvent.PublishAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish event for routing key '{RoutingKey}'", queuedEvent.RoutingKey);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _queue.Writer.TryComplete();
            return base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoutingKey"></param>
        /// <param name="PublishAsync"></param>
        private sealed record QueuedEvent(
            string RoutingKey,
            Func<CancellationToken, Task> PublishAsync);
    }
}
