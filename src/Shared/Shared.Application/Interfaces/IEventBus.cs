using Shared.Domain.Abstractions;

namespace Shared.Application.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : IDomainEvent;
    }
}
