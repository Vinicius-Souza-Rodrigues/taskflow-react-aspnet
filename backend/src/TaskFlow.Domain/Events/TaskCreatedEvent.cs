namespace TaskFlow.Domain.Events;

public sealed record TaskCreatedEvent(int TaskId, string Title) : IDomainEvent;
