namespace TaskFlow.Api.Contracts.Responses;

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
