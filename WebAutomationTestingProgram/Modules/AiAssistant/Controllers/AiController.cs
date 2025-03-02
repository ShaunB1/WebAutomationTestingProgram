using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAutomationTestingProgram.Infrastructure.ExternalApis;
using WebAutomationTestingProgram.Modules.AiAssistant.Models;

namespace WebAutomationTestingProgram.Modules.AiAssistant.Controllers;

[Authorize]
[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly AiService _aiService;

    public AiController(AiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> GenerateResponse([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Prompt cannot be empty.");
        }
        
        var response = await _aiService.GetResponseAsync(request.Message);
        return Ok(response);
    }
}