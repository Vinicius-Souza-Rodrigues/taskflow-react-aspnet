using MediatR;
using TaskFlow.Application.Common;

namespace TaskFlow.Application.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(int Id) : IRequest<Result>;
