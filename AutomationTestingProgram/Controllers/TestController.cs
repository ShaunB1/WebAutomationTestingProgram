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
    private readonly HandleTestPlan _planHandler;
    private readonly HandleTestCase _caseHandler;
    private readonly bool _reportToDevops;

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings)
    {
        _azureDevOpsSettings = azureDevOpsSettings.Value;
        _planHandler = new HandleTestPlan();
        _caseHandler = new HandleTestCase();
        _reportToDevops = false;
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
        
        var executor = new TestExecutor(browser);

        try
        {
            var environment = "EarlyON";
            var fileName = Path.GetFileNameWithoutExtension(excelFilePath);
            var reportHandler = new HandleReporting();

            if (_reportToDevops)
            {
                await reportHandler.ReportToDevOps(browser, testSteps, environment, fileName);                
            }
            else
            {
                await executor.ExecuteTestCasesAsync(browser, testSteps, environment, fileName);
            }
            
            return Ok("Tests executed successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occured during test execution.");
        }
    }
}