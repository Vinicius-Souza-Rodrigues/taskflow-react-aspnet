using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler(ITaskRepository taskRepository, IDomainEventDispatcher domainEventDispatcher)
    : IRequestHandler<UpdateTaskCommand, Result<TaskItem>>
{
    public async Task<Result<TaskItem>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.FindByIdAsync(request.Id);
        if (task is null)
            return Result<TaskItem>.NotFound();

        var notification = new Notification();

        var status = TaskStatusMapper.ResolveRequired(request.Status, notification);
        var title = TaskTitle.Create(request.Title, notification);
        var description = TaskDescription.Create(request.Description, notification);

        if (!notification.IsValid)
            return Result<TaskItem>.Invalid(notification.Errors);

        task.Update(title!, description!, status);
        await taskRepository.SaveChangesAsync();

        await domainEventDispatcher.DispatchAndClearAsync(task);

        return Result<TaskItem>.Success(task);
    }
}
