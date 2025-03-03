using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAutomationTestingProgram.Core.Controllers;
using WebAutomationTestingProgram.Core.Helpers.Requests;
using WebAutomationTestingProgram.Core.Hubs;
using WebAutomationTestingProgram.Core.Services;
using WebAutomationTestingProgram.Core.Services.Logging;
using WebAutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Requests;
using WebAutomationTestingProgram.Modules.TestRunner.Requests.TestController;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Controllers;

[ApiController]
[Route("api/testv2")]
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

    /// <summary>
    /// Receives api requests to validate files
    /// </summary>
    [Authorize]
    [HttpPost("validate")] 
    public async Task<IActionResult> ValidateRequest([FromForm] ValidationRequestModel model)
    {
        ValidationRequest request = new ValidationRequest(Provider, HttpContext.User);
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
        var request = new ProcessRequest(Provider, _hubContext, _playwright, HttpContext.User, guid, model);
        await CopyFileToFolder(model.File, request.FolderPath);
        var email = HttpContext.User.FindFirst("preferred_username")?.Value;
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
            IClientRequest request = RequestHandler.RetrieveRequest(model.ID);

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
            IClientRequest request = RequestHandler.RetrieveRequest(model.ID);

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