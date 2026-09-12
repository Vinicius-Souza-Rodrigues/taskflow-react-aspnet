using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Requests;
using TaskFlow.Api.Contracts.Responses;
using TaskFlow.Api.Mapping;
using TaskFlow.Application.Common;
using TaskFlow.Application.Tasks.Commands.CreateTask;
using TaskFlow.Application.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Tasks.Commands.UpdateTask;
using TaskFlow.Application.Tasks.Queries.GetTaskById;
using TaskFlow.Application.Tasks.Queries.ListTasks;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var result = await _mediator.Send(new CreateTaskCommand(request.Title, request.Description, request.Status));

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        var task = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, TaskResponseMapper.ToResponse(task));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
    {
        var tasks = await _mediator.Send(new ListTasksQuery());
        return Ok(tasks.Select(TaskResponseMapper.ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id));

        return result.Status switch
        {
            ResultStatus.Success => Ok(TaskResponseMapper.ToResponse(result.Value!)),
            _ => NotFound()
        };
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
    {
        var result = await _mediator.Send(new UpdateTaskCommand(id, request.Title, request.Description, request.Status));

        return result.Status switch
        {
            ResultStatus.Success => Ok(TaskResponseMapper.ToResponse(result.Value!)),
            ResultStatus.NotFound => NotFound(),
            _ => BadRequest(new { errors = result.Errors })
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTaskCommand(id));

        return result.Status switch
        {
            ResultStatus.Success => NoContent(),
            ResultStatus.NotFound => NotFound(),
            _ => BadRequest(new { errors = result.Errors })
        };
    }
}
