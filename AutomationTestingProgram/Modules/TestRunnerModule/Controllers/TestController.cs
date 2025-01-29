using AutomationTestingProgram.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AutomationTestingProgram.Modules.TestRunnerModule;

[ApiController]
[Route("api/[controller]")]
public class TestController : CoreController
{
    private readonly IHubContext<TestHub> _hubContext;
    private readonly PlaywrightObject _playwright;

    public TestController(ICustomLoggerProvider provider, PlaywrightObject playwright, IHubContext<TestHub> hubContext):
        base(provider) 
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
     * curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\USER_REGRESSION.xlsx" -F "Type=Chrome" -F "Version=92" -F "Environment=EDCS-9" https://localhost:7117/api/test/run
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
        return await HandleRequest(request);
    }

    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    [Authorize]
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest([FromForm] ProcessRequestModel model)
    {
        ProcessRequest request = new ProcessRequest(_provider, _hubContext, _playwright, HttpContext.User, model);
        await CopyFileToFolder(model.File, request.FolderPath);
        string email = HttpContext.User.FindFirst("preferred_username")!.Value;
        await _hubContext.Clients.User(email).SendAsync("AddGroup", email, request.ID);
        IActionResult result =  await HandleRequest(request);
        await _hubContext.Clients.User(email).SendAsync("RemoveGroup", email, request.ID);
        return result;
    }
}