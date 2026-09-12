namespace TaskFlow.Domain.Events;

public sealed record TaskStatusChangedEvent(int TaskId, TaskItemStatus OldStatus, TaskItemStatus NewStatus) : IDomainEvent;
