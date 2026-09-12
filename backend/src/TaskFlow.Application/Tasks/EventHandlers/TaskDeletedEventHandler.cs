using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Tasks.EventHandlers;

public sealed class TaskDeletedEventHandler(ILogger<TaskDeletedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<TaskDeletedEvent>>
{
    public Task Handle(DomainEventNotification<TaskDeletedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Task {TaskId} deleted", notification.DomainEvent.TaskId);

        return Task.CompletedTask;
    }
}
