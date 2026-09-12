using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler(ITaskRepository taskRepository, IDomainEventDispatcher domainEventDispatcher)
    : IRequestHandler<CreateTaskCommand, Result<TaskItem>>
{
    public async Task<Result<TaskItem>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification();

        var status = TaskStatusMapper.ResolveOptional(request.Status, notification);
        var title = TaskTitle.Create(request.Title, notification);
        var description = TaskDescription.Create(request.Description, notification);

        if (!notification.IsValid)
            return Result<TaskItem>.Invalid(notification.Errors);

        var task = TaskItem.Create(title!, description!, status);
        await taskRepository.AddAsync(task);
        await taskRepository.SaveChangesAsync();

        task.NotifyCreated();
        await domainEventDispatcher.DispatchAndClearAsync(task);

        return Result<TaskItem>.Success(task);
    }
}
