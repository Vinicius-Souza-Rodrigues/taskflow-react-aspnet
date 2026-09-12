namespace TaskFlow.Domain;

// Marker interface only — intentionally has zero dependency on any dispatching
// mechanism (e.g. MediatR). The Domain must not know how its events get delivered.
public interface IDomainEvent
{
}
