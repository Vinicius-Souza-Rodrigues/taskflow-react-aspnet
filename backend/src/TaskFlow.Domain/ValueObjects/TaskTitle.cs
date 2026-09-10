namespace TaskFlow.Domain.ValueObjects;

public sealed class TaskTitle
{
    public const int MaxLength = 200;

    public string Value { get; }

    private TaskTitle(string value) => Value = value;

    public static TaskTitle? Create(string value, Notification notification)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            notification.AddError("Title is required.");
            return null;
        }

        if (value.Length > MaxLength)
        {
            notification.AddError($"Title must be at most {MaxLength} characters.");
            return null;
        }

        return new TaskTitle(value);
    }

    public static TaskTitle Restore(string value) => new(value);
}
