using TaskFlow.Domain;

namespace TaskFlow.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearAsync(IAggregateRoot aggregate);
}
