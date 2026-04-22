using WorkTracker.Common.IntegrationEvents;

namespace WorkTracker.Common.Interfaces
{
    public interface IBackgroundEventPublisher
    {
        Task QueueAsync<TData>(CloudEvent<TData> cloudEvent, string routingKey, CancellationToken cancellationToken = default);
    }
}
