using MediatR;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Queries.ListTasks;

public sealed class ListTasksQueryHandler(ITaskRepository taskRepository) : IRequestHandler<ListTasksQuery, IReadOnlyList<TaskItem>>
{
    public Task<IReadOnlyList<TaskItem>> Handle(ListTasksQuery request, CancellationToken cancellationToken) =>
        taskRepository.ListAllAsync();
}
