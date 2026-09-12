using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Tasks.EventHandlers;

public sealed class TaskCreatedEventHandler(ILogger<TaskCreatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<TaskCreatedEvent>>
{
    public Task Handle(DomainEventNotification<TaskCreatedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Task {TaskId} created: {Title}",
            notification.DomainEvent.TaskId,
            notification.DomainEvent.Title);

        return Task.CompletedTask;
    }
}
