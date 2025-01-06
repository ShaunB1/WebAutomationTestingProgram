using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentsController : ControllerBase
{
    private readonly ILogger<EnvironmentsController> _logger;

    public EnvironmentsController(ILogger<EnvironmentsController> logger)
    {
        //_keychainFilePath = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build()["KeychainFilePath"];
        _logger = logger;
    }

    /* API Request Examples:
     * - KeychainAccounts
     * curl -X GET -H "Content-Type: application/json" http://localhost:5223/api/environments/keychainAccounts
     * - SecretKey
     * curl -X GET -H "Content-Type: application/json" http://localhost:5223/api/environments/secretKey?email=example@example.com
     * - RESET
     * curl -X POST -H "Content-Type: application/json" -d "{\"Email\": \"iam_ma@ontarioemail.ca\"}" http://localhost:5223/api/environments/resetPassword
     * 
     * Test commands:
     * for /l %i in (1,1,10) do start /b curl -X POST -H "Content-Type: application/json" http://localhost:5223/api/test/retrieve
     */

    private async Task<IActionResult> HandleRequest<TRequest>(TRequest request) where TRequest : IClientRequest
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

    [Authorize]
    [HttpGet("keychainAccounts")]
    public async Task<IActionResult> GetKeychainAccounts()
    {
        KeyChainRetrievalRequest request = new KeyChainRetrievalRequest(HttpContext.User);
        return await HandleRequest<KeyChainRetrievalRequest>(request);
    }

    [Authorize]
    [HttpGet("secretKey")]
    public async Task<IActionResult> GetSecretKey([FromQuery] SecretKeyRetrievalRequestModel model)
    {
        SecretKeyRetrievalRequest request = new SecretKeyRetrievalRequest(HttpContext.User, model.Email);
        return await HandleRequest<SecretKeyRetrievalRequest>(request);
    }

    [Authorize]
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequestModel model)
    {
        PasswordResetRequest request = new PasswordResetRequest(HttpContext.User, model.Email);
        return await HandleRequest<PasswordResetRequest>(request);
    }
}