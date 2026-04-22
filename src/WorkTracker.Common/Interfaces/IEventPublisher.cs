using WorkTracker.Common.IntegrationEvents;

namespace WorkTracker.Common.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<TData>(CloudEvent<TData> cloudEvent, string routingKey, CancellationToken cancellationToken = default);
    }
}
