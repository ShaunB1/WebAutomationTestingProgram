using AutomationTestingProgram.Models;
using AutomationTestingProgram.Actions;
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
    private readonly string _keychainFilePath;

    public EnvironmentsController()
    {
        _keychainFilePath = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build()["KeychainFilePath"];
    }

    [Authorize]
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

    [Authorize]
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