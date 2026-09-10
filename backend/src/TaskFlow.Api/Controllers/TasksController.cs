using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Requests;
using TaskFlow.Api.Contracts.Responses;
using TaskFlow.Api.Mapping;
using TaskFlow.Domain;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;

    public TasksController(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var notification = new Notification();
        var status = ResolveOptionalStatus(request.Status, notification);
        var title = TaskTitle.Create(request.Title, notification);
        var description = TaskDescription.Create(request.Description, notification);

        if (!notification.IsValid)
            return BadRequest(new { errors = notification.Errors });

        var task = TaskItem.Create(title!, description!, status);
        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, TaskResponseMapper.ToResponse(task));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
    {
        var tasks = await _taskRepository.ListAllAsync();
        return Ok(tasks.Select(TaskResponseMapper.ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var task = await _taskRepository.FindByIdAsync(id);
        if (task is null)
            return NotFound();

        return Ok(TaskResponseMapper.ToResponse(task));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.FindByIdAsync(id);
        if (task is null)
            return NotFound();

        var notification = new Notification();
        var status = ResolveRequiredStatus(request.Status, notification);
        var title = TaskTitle.Create(request.Title, notification);
        var description = TaskDescription.Create(request.Description, notification);

        if (!notification.IsValid)
            return BadRequest(new { errors = notification.Errors });

        task.Update(title!, description!, status);
        await _taskRepository.SaveChangesAsync();

        return Ok(TaskResponseMapper.ToResponse(task));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _taskRepository.FindByIdAsync(id);
        if (task is null)
            return NotFound();

        await _taskRepository.RemoveAsync(task);
        await _taskRepository.SaveChangesAsync();

        return NoContent();
    }

    private static TaskItemStatus ResolveOptionalStatus(string? value, Notification notification)
    {
        if (string.IsNullOrEmpty(value))
            return TaskItemStatus.Todo;

        if (TaskStatusMapper.TryParse(value, out var status))
            return status;

        notification.AddError($"Invalid status: {value}");
        return TaskItemStatus.Todo;
    }

    private static TaskItemStatus ResolveRequiredStatus(string? value, Notification notification)
    {
        if (TaskStatusMapper.TryParse(value, out var status))
            return status;

        notification.AddError($"Invalid status: {value}");
        return TaskItemStatus.Todo;
    }
}
