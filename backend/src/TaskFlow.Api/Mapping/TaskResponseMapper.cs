using TaskFlow.Api.Contracts.Responses;
using TaskFlow.Domain;

namespace TaskFlow.Api.Mapping;

public static class TaskResponseMapper
{
    public static TaskResponse ToResponse(TaskItem task) => new(
        task.Id,
        task.Title.Value,
        task.Description?.Value,
        TaskStatusMapper.ToWireValue(task.Status),
        task.CreatedAt,
        task.UpdatedAt);
}
