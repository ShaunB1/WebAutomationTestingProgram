using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentsController : ControllerBase
{
    private readonly AzureKeyVaultService _azureKeyVaultService;
    private readonly IWebHostEnvironment _env;
    public EnvironmentsController(AzureKeyVaultService azureKeyVaultService, IWebHostEnvironment env)
    {
        _azureKeyVaultService = azureKeyVaultService;
        _env = env;
    }

    [HttpGet("keychainAccounts")]
    public async Task<IActionResult> GetKeychainAccounts()
    {
        string contentRootPath = _env.ContentRootPath;
        string filepath = Path.Combine(contentRootPath, "resources", "KeychainAccounts2023.xls");

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

    [HttpGet("secretKey")]
    public async Task<IActionResult> GetSecretKey([FromQuery] string email)
    {
        try
        {
            string secretKey = _azureKeyVaultService.GetKvSecret(email);
            if (secretKey.StartsWith("Error"))
            {
                Console.WriteLine("Secret key could not be fetched from Key Vault");
                return StatusCode(StatusCodes.Status400BadRequest, new { error = secretKey });
            }
            Console.WriteLine("Successfully read secret from Azure Key Vault");
            return Ok(new { secretKey });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}