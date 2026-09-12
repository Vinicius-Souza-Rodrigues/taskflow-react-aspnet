using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler(ITaskRepository taskRepository) : IRequestHandler<GetTaskByIdQuery, Result<TaskItem>>
{
    public async Task<Result<TaskItem>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.FindByIdAsync(request.Id);
        return task is null ? Result<TaskItem>.NotFound() : Result<TaskItem>.Success(task);
    }
}
