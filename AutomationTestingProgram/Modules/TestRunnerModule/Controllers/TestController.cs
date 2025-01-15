using AutomationTestingProgram.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationTestingProgram.Modules.TestRunnerModule;

[ApiController]
[Route("api/[controller]")]
public class TestController : CoreController
{
    public TestController(ICustomLoggerProvider provider):
        base(provider) { }

    /* API Request Examples:
     * - VALIDATE
     *   curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" http://localhost:5223/api/test/validate
     *   curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\CGRT_REGRESSION.xlsx" https://localhost:7117/api/test/validate
     * - PROCESS
     * curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" -F "Type=Chrome" -F "Version=92" -F "Environment=TestEnv" http://localhost:5223/api/test/run
     */

    /// <summary>
    /// Receives api requests to validate files
    /// </summary>
    [Authorize]
    [HttpPost("validate")] 
    public async Task<IActionResult> ValidateRequest([FromForm] ValidationRequestModel model)
    {
        ValidationRequest request = new ValidationRequest(_provider, HttpContext.User, model.File);
        return await HandleRequest(request);
    }

    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    [Authorize]
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest([FromForm] ProcessRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ProcessRequest request = new ProcessRequest(_provider, HttpContext.User, model.File, model.Type, model.Version, model.Environment);
        return await HandleRequest(request);
    }
}