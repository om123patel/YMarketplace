using Shared.Domain.Abstractions;

namespace Shared.Application.Interfaces
{
    public interface IEventHandler<T> where T : IDomainEvent
    {
        Task HandleAsync(T @event, CancellationToken cancellationToken = default);
    }
}
