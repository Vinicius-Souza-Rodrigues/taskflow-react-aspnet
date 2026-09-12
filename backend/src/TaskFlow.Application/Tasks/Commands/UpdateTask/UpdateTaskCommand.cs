using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(int Id, string Title, string? Description, string? Status) : IRequest<Result<TaskItem>>;
