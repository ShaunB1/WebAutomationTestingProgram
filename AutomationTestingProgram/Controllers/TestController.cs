using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AzureDevOpsSettings _azureDevOpsSettings;

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings)
    {
        _azureDevOpsSettings = azureDevOpsSettings.Value;
    }
    
    [HttpPost("run")]
    public async Task<IActionResult> RunTests([FromBody] string excelFilePath)
    {
        var excelReader = new ExcelReader();
        var testSteps = excelReader.ReadTestSteps(excelFilePath);
        
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Channel = "chrome"
        });

        var reporter = new AzureDevOpsReporter(
            _azureDevOpsSettings.Url,
            _azureDevOpsSettings.Pat,
            _azureDevOpsSettings.ProjectName
        );

        await reporter.DeleteTestPlan("Test Environment");
        await reporter.DeleteTestCasesAsync("Shaun Bautista");
        Console.Write("DELETED TEST CASES");
        var executor = new TestExecutor(browser, reporter);

        try
        {
            var environment = "Test Environment";
            var fileName = Path.GetFileNameWithoutExtension(excelFilePath);
            
            await executor.ExecuteTestCasesAsync(testSteps, environment, fileName);
            
            return Ok("Tests executed successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occured during test execution.");
        }
    }
}