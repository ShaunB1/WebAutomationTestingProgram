using System.Web.Http;
using AutomationTestingProgram.Modules.DBConnector.Models;
using AutomationTestingProgram.Modules.DBConnector.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutomationTestingProgram.Modules.DBConnector.Controllers;

[Authorize]
[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class WorkersController : ControllerBase
{
    private readonly IWorkersService _service;

    public WorkersController(IWorkersService service)
    {
        _service = service;
    }

    [Microsoft.AspNetCore.Mvc.HttpGet]
    public async Task<IActionResult> GetAllWorkers()
    {
        var workers = await _service.GetAllWorkersAsync();
        return Ok(workers);
    }

    [Microsoft.AspNetCore.Mvc.HttpPost]
    public async Task<IActionResult> AddWorker([Microsoft.AspNetCore.Mvc.FromBody] WorkerModel worker)
    {
        try
        {
            var addedWorker = await _service.AddWorkerAsync(worker);
            return CreatedAtAction(nameof(GetAllWorkers), new { id = addedWorker.Id }, addedWorker);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Microsoft.AspNetCore.Mvc.HttpDelete]
    public async Task<IActionResult> DeleteWorker([Microsoft.AspNetCore.Mvc.FromBody] WorkerDeleteRequest request)
    {
        var deletedWorker = await _service.DeleteWorkerAsync(request);

        if (deletedWorker == null)
        {
            return NotFound(new { message = "Worker not found." });
        }
        
        return Ok(deletedWorker);
    }
}