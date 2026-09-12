namespace TaskFlow.Domain.Events;

public sealed record TaskDeletedEvent(int TaskId) : IDomainEvent;
