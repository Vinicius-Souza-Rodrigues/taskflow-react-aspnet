using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskFlow.Api.Contracts.Requests;
using TaskFlow.Api.Contracts.Responses;

namespace TaskFlow.Api.Tests;

public class TasksApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TasksApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Crud_HappyPath_WorksEndToEnd()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/tasks", new CreateTaskRequest("Smoke test task", "created by xunit", null));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.Equal("Smoke test task", created!.Title);
        Assert.Equal("TODO", created.Status);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<TaskResponse>>();
        Assert.Contains(list!, t => t.Id == created.Id);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/tasks/{created.Id}",
            new UpdateTaskRequest("Smoke test task (updated)", "updated by xunit", "IN_PROGRESS"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal("IN_PROGRESS", updated!.Status);
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("", null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidStatus_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/tasks", new CreateTaskRequest("Valid title", null, "NOT_A_STATUS"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/tasks/999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/tasks/999999999", new UpdateTaskRequest("Title", null, "TODO"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
