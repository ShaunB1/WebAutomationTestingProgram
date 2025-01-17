using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationTestingProgram.Core;

[ApiController]
[Route("api/[controller]")]
public class CoreController : ControllerBase
{
    protected readonly ICustomLoggerProvider _provider;
    private readonly ICustomLogger _logger;

    public CoreController(ICustomLoggerProvider provider)
    {
        _provider = provider;
        _logger = _provider.CreateLogger<CoreController>();
    }

    protected async Task<IActionResult> HandleRequest<TRequest>(TRequest request) where TRequest : IClientRequest
    {
        try
        {
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) received.");

            await RequestHandler.ProcessAsync(request);

            // If request succeeds
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) successfully completed.");
            return Ok(new { Message = $"Completed", Request = request });
        }
        catch (OperationCanceledException e)
        {
            // If request cancelled
            _logger.LogWarning($"{request.GetType().Name} (ID: {request.ID}) cancelled.\nMessage: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"{request.GetType().Name} (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
    }

    /* API Request Examples:
     * - STOP
     * curl -X POST -H "Content-Type: application/json" -d "{\"ID\": \"request-id-to-stop\"}" https://localhost:7117/api/core/stop
     * - GETACTIVEREQUESTS
     * 
     * TYPE: curl -X POST -H "Content-Type: application/json" -d "{\"FilterType\": \"Type\", \"FilterValue\": \"AutomationTestingProgram.Core.RetrievalRequest\"}" https://localhost:7117/api/core/retrieve
     * ID: curl -X POST -H "Content-Type: application/json" -d "{\"FilterType\": \"ID\", \"FilterValue\": \"dfe82f6d-c5e2-4a44-acfd-a726dda2ae5f\"}" https://localhost:7117/api/core/retrieve
     * NONE: curl -X POST -H "Content-Type: application/json" -d "{\"FilterType\": \"None\", \"FilterValue\": \"asdasd\"}" https://localhost:7117/api/core/retrieve
     * 
     * 
     * Test commands:
     * for /l %i in (1,1,10) do start /b curl -X POST -H "Content-Type: application/json" http://localhost:5223/api/core/retrieve
     * 
     */

    /// <summary>
    /// Receives api requests to retrieve all active requests.
    /// </summary>
    [Authorize]
    [HttpPost("retrieve")]
    public async Task<IActionResult> GetActiveRequests([FromBody] RetrievalRequestModel model)
    {        
        RetrievalRequest request = new RetrievalRequest(_provider, HttpContext.User, model.FilterType, model.FilterValue);
        return await HandleRequest(request);
    }

    /// <summary>
    /// Receives api requests to stop execution of another request
    /// </summary>
    [Authorize]
    [HttpPost("stop")]
    public async Task<IActionResult> StopRequest([FromBody] CancellationRequestModel model)
    {
        CancellationRequest request = new CancellationRequest(_provider, HttpContext.User, model.ID);
        return await HandleRequest(request);
    }
}

