using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<T>(
            T @event,
            CancellationToken cancellationToken = default)
            where T : IDomainEvent
        {
            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider
                .GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
        }
    }
}
