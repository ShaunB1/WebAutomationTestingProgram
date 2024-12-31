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

    private async Task<IActionResult> HandleRequest<TRequest>(TRequest request) where TRequest : IClientRequest
    {
        try
        {
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) received.");

            await RequestHandler.ProcessRequestAsync(request);

            // If request succeeds
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) successfully completed.");
            return Ok(new { Message = $"{request.GetType().Name} (ID: {request.ID}) Complete.", Request = request });
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

    //[Authorize]
    [HttpGet("keychainAccounts")]
    public async Task<IActionResult> GetKeychainAccounts()
    {
        string baseDirectory = AppContext.BaseDirectory;
        string filepath = _keychainFilePath.Replace("%PROJECT_ROOT%", baseDirectory);
        Console.WriteLine(filepath);

        var keychainRows = new List<object>();
        try
        {
            await using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new HSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheetAt(0);

                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++)
                {
                    IRow row = sheet.GetRow(rowIdx);
                    if (row != null)
                    {
                        keychainRows.Add(new
                        {
                            email = row.GetCell(14)?.StringCellValue ?? string.Empty,
                            role = row.GetCell(18)?.StringCellValue ?? string.Empty,
                            organization = row.GetCell(19)?.StringCellValue ?? string.Empty
                        });
                    }
                }
            }
            Console.WriteLine("Successfully read Keychain file");
            string json = JsonSerializer.Serialize(keychainRows);
            return Ok(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    //[Authorize]
    [HttpGet("secretKey")]
    public async Task<IActionResult> GetSecretKey([FromQuery] string email)
    {
        var result = await AzureKeyVaultService.GetKvSecret(email);
        Console.WriteLine(result.message);
        if (!result.success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.message });
        }
        return Ok(new { result.message });
    }

    [Authorize]
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] string email)
    {

        var result = await PasswordResetService.ResetPassword(email);
        if (!result.success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.message, email = email, success = false });
        }
        return Ok(new { message = result.message, email = email, success = true });
    }
}