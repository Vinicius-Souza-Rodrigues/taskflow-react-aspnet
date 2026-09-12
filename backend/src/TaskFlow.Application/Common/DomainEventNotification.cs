using MediatR;
using TaskFlow.Domain;

namespace TaskFlow.Application.Common;

// Bridges a pure TaskFlow.Domain event into MediatR's notification pipeline. The Domain
// project never references MediatR directly — only the Application layer does.
public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
