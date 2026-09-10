namespace TaskFlow.Domain.ValueObjects;

public sealed class TaskDescription
{
    public const int MaxLength = 2000;

    public string? Value { get; }

    private TaskDescription(string? value) => Value = value;

    public static TaskDescription? Create(string? value, Notification notification)
    {
        if (value is not null && value.Length > MaxLength)
        {
            notification.AddError($"Description must be at most {MaxLength} characters.");
            return null;
        }

        return new TaskDescription(value);
    }

    public static TaskDescription Restore(string? value) => new(value);
}
