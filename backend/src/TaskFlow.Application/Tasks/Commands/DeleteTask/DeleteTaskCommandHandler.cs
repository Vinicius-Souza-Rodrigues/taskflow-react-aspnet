using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler(ITaskRepository taskRepository, IDomainEventDispatcher domainEventDispatcher)
    : IRequestHandler<DeleteTaskCommand, Result>
{
    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.FindByIdAsync(request.Id);
        if (task is null)
            return Result.NotFound();

        task.RaiseDeleted();
        await taskRepository.RemoveAsync(task);
        await taskRepository.SaveChangesAsync();

        // Dispatched only after the delete actually committed — a handler reacting to
        // this event should never see a "deletion" that didn't really happen.
        await domainEventDispatcher.DispatchAndClearAsync(task);

        return Result.Success();
    }
}
