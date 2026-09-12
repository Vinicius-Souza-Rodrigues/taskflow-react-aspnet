using MediatR;
using TaskFlow.Application.Common;
using TaskFlow.Domain;

namespace TaskFlow.Application.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(string Title, string? Description, string? Status) : IRequest<Result<TaskItem>>;
