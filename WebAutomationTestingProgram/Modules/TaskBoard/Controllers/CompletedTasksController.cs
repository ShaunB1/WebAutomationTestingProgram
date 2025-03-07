﻿using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using WebAutomationTestingProgram.Modules.TaskBoard.Models;
using WebAutomationTestingProgram.Modules.TaskBoard.Services;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Controllers;

[Authorize]
[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/completed_tasks")]
public class CompletedTasksController : ControllerBase
{
    private readonly ICompletedTasksService _service;

    public CompletedTasksController(ICompletedTasksService service)
    {
        _service = service;
    }
    
    [Microsoft.AspNetCore.Mvc.HttpGet]
    public async Task<IActionResult> GetAllCompletedTasks()
    {
        var completedTasks = await _service.GetAllCompletedTasksAsync();
        return Ok(completedTasks);
    }

    [Microsoft.AspNetCore.Mvc.HttpPost]
    public async Task<IActionResult> AddCompletedTaskAsync([Microsoft.AspNetCore.Mvc.FromBody] CompletedTaskModel completedTask)
    {
        try
        {
            var addedCompletedTask = await _service.AddCompletedTaskAsync(completedTask);
            return CreatedAtAction(nameof(GetAllCompletedTasks), new { id = completedTask.Id }, addedCompletedTask);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}