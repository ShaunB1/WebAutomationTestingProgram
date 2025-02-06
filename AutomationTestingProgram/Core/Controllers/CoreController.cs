using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutomationTestingProgram.Core;

[ApiController]
[Route("api/[controller]")]
public class CoreController : ControllerBase
{
    protected readonly ICustomLoggerProvider _provider;
    private readonly ICustomLogger _logger;
    private readonly RequestHandler _requestHandler;

    public CoreController(ICustomLoggerProvider provider, RequestHandler handler)
    {
        _provider = provider;
        _logger = _provider.CreateLogger<CoreController>();
        _requestHandler = handler;
    }

    protected async Task<IActionResult> HandleRequest<TRequest, TReturn>(TRequest request, Func<TRequest, Task<TReturn>> getProperty) where TRequest : IClientRequest
    {
        try
        {
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) received.");

            await _requestHandler.ProcessAsync(request);

            var result = await getProperty(request);

            // If request succeeds
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) successfully completed.");
            return Ok(new { Result = result });
        }
        catch (OperationCanceledException e)
        {
            // If request cancelled
            _logger.LogWarning($"{request.GetType().Name} (ID: {request.ID}) cancelled.\nMessage: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"{request.GetType().Name} (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message });
        }
    }

    /// <summary>
    /// Copies the given IFormFile to the request Folder.
    /// </summary>
    /// <param name="file">The provided file.</param>
    /// <param name="folderPath">The path of the folder. </param>
    /// <returns></returns>
    protected async Task CopyFileToFolder(IFormFile file, string folderPath)
    {
        string filePath = Path.Combine(folderPath, file.FileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
        {
            await file.CopyToAsync(fileStream);
        }
    }

    /* API Request Examples:
     * - STOP
     * curl -X POST -H "Content-Type: application/json" -d "{\"ID\": \"f76d4bdd-bccf-4a42-bd91-2c5a1d5c81b8\"}" https://localhost:7117/api/core/stop
     * - GETACTIVEREQUESTS
     * 
     * TYPE: curl -X POST -H "Content-Type: application/json" -d "{\"FilterType\": \"Type\", \"FilterValue\": \"AutomationTestingProgram.Core.RetrievalRequest\"}" https://localhost:7117/api/core/retrieve
     * ID: curl -X POST -H "Content-Type: application/json" -d "{\"FilterType\": \"ID\", \"FilterValue\": \"dfe82f6d-c5e2-4a44-acfd-a726dda2ae5f\"}" https://localhost:7117/api/core/retrieve
     * NONE: curl -X POST -H "Content-Type: application/json" -d "{\"FilterType\": \"None\", \"FilterValue\": \"asdasd\"}" https://localhost:7117/api/core/retrieve
     * 
     * - GETLOGFILES
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
        RetrievalRequest request = new RetrievalRequest(_provider, _requestHandler, HttpContext.User, model);
        return await HandleRequest(request, async (req) =>
        {
            return req.RetrievedRequests;
        });
    }

    /// <summary>
    /// Receives api requests to retrieve page logs of a ProcessRequest
    /// </summary>
    [Authorize]
    [HttpGet("retrieveLogFile")]
    public IActionResult GetRequestLogs([FromQuery] RetrieveLogFileModel model)
    {
        try
        {
            (string, bool) val = LogManager.RetrieveLog(model.ID);
            string logPath = val.Item1;
            if (System.IO.File.Exists(logPath))
            {
                // Registers OnCompleted to relete temp log file after it was sent
                if (val.Item2)
                {
                    HttpContext.Response.OnCompleted(() =>
                    {
                        try
                        {
                            if (System.IO.File.Exists(logPath))
                            {
                                System.IO.File.Delete(logPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete temporary file: {ex.Message}");
                        }
                        return Task.CompletedTask;
                    });
                }

                return PhysicalFile(logPath, "application/octet-stream", "log.txt");
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Error = e.Message });
        }
    }

    /// <summary>
    /// Receives api requests to stop execution of another request
    /// </summary>
    [Authorize]
    [HttpPost("stop")]
    public async Task<IActionResult> StopRequest([FromBody] CancellationRequestModel model)
    {
        CancellationRequest request = new CancellationRequest(_provider, _requestHandler, HttpContext.User, model);
        return await HandleRequest(request, async (req) =>
        {
            return $"Request {req.CancelRequestID} cancelled successfully";
        });
    }
}

