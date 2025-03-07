﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAutomationTestingProgram.Core.Helpers.Requests;
using WebAutomationTestingProgram.Core.Hubs;
using WebAutomationTestingProgram.Core.Models.Requests;
using WebAutomationTestingProgram.Core.Requests;
using WebAutomationTestingProgram.Core.Services;
using WebAutomationTestingProgram.Core.Services.Logging;

namespace WebAutomationTestingProgram.Core.Controllers;

[ApiController]
[Route("api/test")]
public class CoreController : ControllerBase
{
    protected readonly ICustomLoggerProvider Provider;
    protected readonly RequestHandler RequestHandler;
    private readonly ICustomLogger _logger;
    private readonly IHubContext<TestHub> _hubContext;

    public CoreController(ICustomLoggerProvider provider, RequestHandler handler, IHubContext<TestHub> hubContext)
    {
        Provider = provider;
        _logger = Provider.CreateLogger<CoreController>();
        RequestHandler = handler;
        _hubContext = hubContext;
    }

    protected async Task<IActionResult> HandleRequest<TRequest, TReturn>(
        TRequest request, 
        Func<TRequest, Task<TReturn>> getProperty) where TRequest : IClientRequest
    {
        try
        {
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.Id}) received.");

            await RequestHandler.ProcessAsync(request);

            var result = await getProperty(request);

            // If request succeeds
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.Id}) successfully completed.");
            return Ok(new { Result = result });
        }
        catch (OperationCanceledException e)
        {
            // If request cancelled
            _logger.LogWarning($"{request.GetType().Name} (ID: {request.Id}) cancelled.\nMessage: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"{request.GetType().Name} (ID: {request.Id}) failed.\nError: '{e.Message}'");
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
        var filePath = Path.Combine(folderPath, file.FileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
        await file.CopyToAsync(fileStream);
    }
    
    /// <summary>
    /// Receives api requests to retrieve all active requests.
    /// </summary>
    [Authorize]
    [HttpPost("retrieve")]
    public async Task<IActionResult> GetActiveRequests([FromBody] RetrievalRequestModel model)
    {        
        var request = new RetrievalRequest(Provider, RequestHandler, HttpContext.User, model);
        return await HandleRequest(request, (req) =>
        {
            return Task.FromResult(req.RetrievedRequests);
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
        CancellationRequest request = new CancellationRequest(Provider, RequestHandler, HttpContext.User, model);
        string email = HttpContext.User.FindFirst("preferred_username")!.Value;
        await _hubContext.Clients.Groups(model.ID).SendAsync("RunStopped", model.ID, $"User: {email} has stopped Test Run: {model.ID}");
        return await HandleRequest(request, async (req) =>
        {
            return $"Request {req.CancelRequestId} cancelled successfully";
        });
    }
}

