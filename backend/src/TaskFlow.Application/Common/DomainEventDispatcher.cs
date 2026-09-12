using MediatR;
using TaskFlow.Domain;

namespace TaskFlow.Application.Common;

public sealed class DomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public async Task DispatchAndClearAsync(IAggregateRoot aggregate)
    {
        var events = aggregate.DomainEvents.ToArray();
        aggregate.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await mediator.Publish(notification);
        }
    }
}
