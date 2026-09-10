namespace TaskFlow.Api.Contracts.Requests;

public record CreateTaskRequest(string Title, string? Description, string? Status);
