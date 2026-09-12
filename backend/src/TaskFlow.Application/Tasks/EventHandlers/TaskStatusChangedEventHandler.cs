using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Tasks.EventHandlers;

public sealed class TaskStatusChangedEventHandler(ILogger<TaskStatusChangedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<TaskStatusChangedEvent>>
{
    public Task Handle(DomainEventNotification<TaskStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Task {TaskId} status changed: {OldStatus} -> {NewStatus}",
            notification.DomainEvent.TaskId,
            notification.DomainEvent.OldStatus,
            notification.DomainEvent.NewStatus);

        return Task.CompletedTask;
    }
}
