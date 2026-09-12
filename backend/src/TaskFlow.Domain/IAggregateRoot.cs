namespace TaskFlow.Domain;

// Marker interface: identifies which entities are Aggregate Roots (the only kind of
// entity the outside world is allowed to load/save directly through a repository).
public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
