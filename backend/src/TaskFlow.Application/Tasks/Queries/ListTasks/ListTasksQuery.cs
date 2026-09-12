using MediatR;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Queries.ListTasks;

public sealed record ListTasksQuery : IRequest<IReadOnlyList<TaskItem>>;
