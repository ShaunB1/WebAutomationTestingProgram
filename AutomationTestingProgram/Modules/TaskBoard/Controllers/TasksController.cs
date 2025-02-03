using System.Web.Http;
using AutomationTestingProgram.Modules.DBConnector.Models;
using AutomationTestingProgram.Modules.DBConnector.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutomationTestingProgram.Modules.DBConnector.Controllers;

[Authorize]
[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITasksService _service;

    public TasksController(ITasksService service)
    {
        _service = service;
    }
    
    [Microsoft.AspNetCore.Mvc.HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _service.GetAllTasksAsync();
        return Ok(tasks);
    }

    [Microsoft.AspNetCore.Mvc.HttpPost]
    public async Task<IActionResult> AddTask([Microsoft.AspNetCore.Mvc.FromBody] TaskModel task)
    {
        try
        {
            var addedTask = await _service.AddTaskAsync(task);
            return CreatedAtAction(nameof(GetAllTasks), new { id = addedTask.Id }, addedTask);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Microsoft.AspNetCore.Mvc.HttpPatch]
    public async Task<IActionResult> PatchTaskAsync([Microsoft.AspNetCore.Mvc.FromBody] TaskPatchRequest request)
    {
        var updatedTask = await _service.PatchTaskAsync(request.DraggableId, request.Name, request.Description, request.Priority);

        if (updatedTask == null)
        {
            return NotFound(new { message = "Task not found." });
        }
        
        return Ok(updatedTask);
    }

    [Microsoft.AspNetCore.Mvc.HttpPut]
    public async Task<IActionResult> PutTaskAsync([Microsoft.AspNetCore.Mvc.FromBody] TaskPutRequest request)
    {
        var updatedTask = await _service.PutTaskAsync(request.DraggableId, request.DestinationDroppableId, request.StartDate);

        if (updatedTask == null)
        {
            return NotFound(new { message = "Task not found." });
        }
        
        return Ok(updatedTask);
    }

    [Microsoft.AspNetCore.Mvc.HttpDelete]
    public async Task<IActionResult> DeleteTaskAsync([Microsoft.AspNetCore.Mvc.FromBody] TaskDeleteRequest request)
    {
        var deletedTask = await _service.DeleteTaskAsync(request.DraggableId);

        if (deletedTask == null)
        {
            return NotFound(new { message = "Task not found." });
        }
        
        return Ok(deletedTask);
    }
}