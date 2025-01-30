﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentsController : CoreController
{
    private readonly PasswordResetService _passwordResetService;
    private readonly AzureKeyVaultService _azureKeyVaultService;
    private readonly string _keyChainFileName;

    public EnvironmentsController(ICustomLoggerProvider provider, RequestHandler handler, IOptions<PathSettings> options, PasswordResetService passwordResetService, AzureKeyVaultService azureKeyVaultService)
        :base(provider, handler)
    {
        _passwordResetService = passwordResetService;
        _azureKeyVaultService = azureKeyVaultService;
        _keyChainFileName = options.Value.KeychainFilePath;
    }

    /* API Request Examples:
     * - KeychainAccounts
     * curl -X GET -H "Content-Type: application/json" https://localhost:7117/api/environments/keychainAccounts
     * - SecretKey
     * curl -X GET -H "Content-Type: application/json" https://localhost:7117/api/environments/secretKey?email=example@example.com
     * - RESET
     * curl -X POST -H "Content-Type: application/json" -d "{\"Email\": \"iam_ma@ontarioemail.ca\"}" https://localhost:7117/api/environments/resetPassword
     * 
     * Test commands:
     * for /l %i in (1,1,10) do start /b curl -X POST -H "Content-Type: application/json" http://localhost:5223/api/test/retrieve
     */

    [Authorize]
    [HttpGet("keychainAccounts")]
    [ResponseCache(Duration = 14400, Location = ResponseCacheLocation.Client)] // Cached for four hours
    public async Task<IActionResult> GetKeychainAccounts()
    {
        KeyChainRetrievalRequest request = new KeyChainRetrievalRequest(_provider, HttpContext.User, _keyChainFileName);
        return await HandleRequest(request, async (req) =>
        {
            return req.Accounts;
        });
    }

    [Authorize]
    [HttpGet("secretKey")]
    public async Task<IActionResult> GetSecretKey([FromQuery] SecretKeyRetrievalRequestModel model)
    {
        SecretKeyRetrievalRequest request = new SecretKeyRetrievalRequest(_provider, _azureKeyVaultService, HttpContext.User, model);
        return await HandleRequest(request, async (req) =>
        {
            return req.SecretKey;
        });
    }

    [Authorize]
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequestModel model)
    {
        PasswordResetRequest request = new PasswordResetRequest(_provider, _passwordResetService, HttpContext.User, model);
        return await HandleRequest(request, async (req) =>
        {
            return $"Password for {req.Email} reset successfully";
        });
    }
}