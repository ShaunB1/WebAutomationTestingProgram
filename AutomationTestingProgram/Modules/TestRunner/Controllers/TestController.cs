using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Services;
using AutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController;
using AutomationTestingProgram.Modules.TestRunner.Models.Requests;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AutomationTestingProgram.Modules.TestRunner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : CoreController
{
    private readonly IHubContext<TestHub> _hubContext;
    private readonly PlaywrightObject _playwright;

    public TestController(ICustomLoggerProvider provider, RequestHandler handler, PlaywrightObject playwright, IHubContext<TestHub> hubContext):
        base(provider, handler, hubContext) 
    { 
        _playwright = playwright;
        _hubContext = hubContext;
    }

    /* API Request Examples:
     * - VALIDATE
     *   curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" http://localhost:5223/api/test/validate
     *   curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\CGRT_REGRESSION.xlsx" https://localhost:7117/api/test/validate
     * - PROCESS
     * curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" -F "Type=Chrome" -F "Version=92" -F "Environment=TestEnv" http://localhost:5223/api/test/run
     * curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\EarlyON_Regression_Test.xlsx" -F "Type=Chrome" -F "Version=92" -F "Environment=EarlyON-AAD" -F "Delay=1" -F "TestRunID=dfe82f6d-c5e2-4a44-acfd-a726dda2ae5f" https://localhost:7117/api/test/run
     * "C:\Users\DobrinD\Downloads\EarlyON_Regression_Test.xlsx"
     */

    /// <summary>
    /// Receives api requests to validate files
    /// </summary>
    [Authorize]
    [HttpPost("validate")] 
    public async Task<IActionResult> ValidateRequest([FromForm] ValidationRequestModel model)
    {
        ValidationRequest request = new ValidationRequest(_provider, HttpContext.User);
        await CopyFileToFolder(model.File, request.FolderPath);
        return await HandleRequest(request, async (req) =>
        {
            return "Validation successful";
        });
    }

    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    [Authorize]
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest([FromForm] ProcessRequestModel model)
    {
        var guid = Guid.NewGuid().ToString();
        var request = new ProcessRequest(_provider, _hubContext, _playwright, HttpContext.User, guid, model);
        await CopyFileToFolder(model.File, request.FolderPath);
        var email = HttpContext.User.FindFirst("preferred_username")!.Value;
        await _hubContext.Clients.All.SendAsync("NewRun", guid, $"User: {email} has created Test Run: {guid}");

        // Run test asynchronously. Don't await, so user can get the GUID for SignalR connection
        HandleRequest(request, (req) => Task.FromResult("Run Successful"));
        return Ok(new { Result=guid });
    }

    /// <summary>
    /// Receives api requests to pause ProcessRequest
    /// </summary>
    [Authorize]
    [HttpPost("pause")]
    public async Task<IActionResult> PauseRequest([FromQuery] PauseRequestModel model)
    {
        string email = HttpContext.User.FindFirst("preferred_username")!.Value;
        try
        {
            IClientRequest request = _requestHandler.RetrieveRequest(model.ID);

            if (request is ProcessRequest processRequest)
            {
                processRequest.Pause();
                await _hubContext.Clients.Group(model.ID).SendAsync("RunPaused", model.ID, $"User: {email} has paused Test Run: {model.ID}");
                return Ok(new { Result = $"Request {model.ID} paused successfully" });
            }
            else
            {
                throw new Exception($"Request (ID: {model.ID}) cannot be paused (invalid type).");
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Error = e.Message });
        }
    }

    /// <summary>
    /// Receives api requests to unpause ProcessRequest
    /// </summary>
    [Authorize]
    [HttpPost("unpause")]
    public async Task<IActionResult> UnPauseRequest([FromQuery] UnPauseRequestModel model)
    {
        string email = HttpContext.User.FindFirst("preferred_username")!.Value;
        try
        {
            IClientRequest request = _requestHandler.RetrieveRequest(model.ID);

            if (request is ProcessRequest processRequest)
            {
                processRequest.Unpause();
                await _hubContext.Clients.Group(model.ID).SendAsync("RunUnpaused", model.ID, $"User: {email} has unpaused Test Run: {model.ID}");
                return Ok(new { Result = $"Request {model.ID} unpaused successfully" });
            }
            else
            {
                throw new Exception($"Request (ID: {model.ID}) cannot be unpaused (invalid type).");
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Error = e.Message });
        }
    }
}