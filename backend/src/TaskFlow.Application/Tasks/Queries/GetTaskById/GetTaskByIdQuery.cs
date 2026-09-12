using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(int Id) : IRequest<Result<TaskItem>>;
