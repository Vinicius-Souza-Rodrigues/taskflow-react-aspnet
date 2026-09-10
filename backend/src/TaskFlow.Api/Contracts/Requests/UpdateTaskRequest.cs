namespace TaskFlow.Api.Contracts.Requests;

public record UpdateTaskRequest(string Title, string? Description, string Status);
