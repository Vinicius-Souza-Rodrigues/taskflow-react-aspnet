using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain;

// Named TaskItem instead of "Task" to avoid colliding with System.Threading.Tasks.Task,
// which every async method in the Api and Infra projects already uses.
public sealed class TaskItem
{
    public int Id { get; private set; }
    public TaskTitle Title { get; private set; } = null!;
    // Nullable to match how EF Core actually materializes this column: when the stored
    // value is NULL, EF Core assigns null directly and skips the value converter.
    public TaskDescription? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TaskItem()
    {
    }

    public static TaskItem Create(TaskTitle title, TaskDescription description, TaskItemStatus status)
    {
        var now = DateTime.UtcNow;

        return new TaskItem
        {
            Title = title,
            Description = description,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(TaskTitle title, TaskDescription description, TaskItemStatus status)
    {
        Title = title;
        Description = description;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
