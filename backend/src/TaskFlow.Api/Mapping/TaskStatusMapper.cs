using TaskFlow.Domain;

namespace TaskFlow.Api.Mapping;

public static class TaskStatusMapper
{
    public static bool TryParse(string? value, out TaskItemStatus status)
    {
        switch (value)
        {
            case "TODO":
                status = TaskItemStatus.Todo;
                return true;
            case "IN_PROGRESS":
                status = TaskItemStatus.InProgress;
                return true;
            case "DONE":
                status = TaskItemStatus.Done;
                return true;
            default:
                status = default;
                return false;
        }
    }

    public static string ToWireValue(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Todo => "TODO",
        TaskItemStatus.InProgress => "IN_PROGRESS",
        TaskItemStatus.Done => "DONE",
        _ => throw new InvalidOperationException($"Unmapped status: {status}")
    };
}
