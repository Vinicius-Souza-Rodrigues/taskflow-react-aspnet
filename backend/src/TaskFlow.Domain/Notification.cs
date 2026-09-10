namespace TaskFlow.Domain;

public sealed class Notification
{
    private readonly List<string> _errors = [];

    public IReadOnlyList<string> Errors => _errors;
    public bool IsValid => _errors.Count == 0;

    public void AddError(string message) => _errors.Add(message);
}
