using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using WebAutomationTestingProgram.Actions;
using WebAutomationTestingProgram.Core.Hubs.Services;
using WebAutomationTestingProgram.Core.Settings.Azure;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Services;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Services.AzureReporter;
using WebAutomationTestingProgram.Modules.TestRunnerV2.Services.File;

namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AzureDevOpsSettings _azureDevOpsSettings;
    private readonly HandleTestPlan _planHandler;
    private readonly HandleTestCase _caseHandler;
    private readonly bool _reportToDevops;
    private readonly ILogger<TestController> _logger;
    private readonly SignalRService _broadcaster;

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings, ILogger<TestController> logger, SignalRService broadcaster)
    {
        _azureDevOpsSettings = azureDevOpsSettings.Value;
        _planHandler = new HandleTestPlan();
        _caseHandler = new HandleTestCase();
        _reportToDevops = true;
        _logger = logger;
        _broadcaster = broadcaster;
    }

    // We want to authorization for all endpoints, but if you are testing then comment out the line below
    [Authorize]
    [HttpPost("run")]
    public async Task<IActionResult> RunTests(
        IFormFile file,
        [FromForm] string env,
        [FromForm] string browser,
        [FromForm] string delay,
        [FromHeader(Name = "TestRunId")] string testRunId
    )
    {
        if (file == null || string.IsNullOrEmpty(testRunId))
        {
            return BadRequest("File or TestRunId missing.");
        }
        
        _broadcaster.BroadcastLog(testRunId, "newrun");
        
        Console.WriteLine("Received test request");
        // Response.ContentType = "text/event-stream";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        try
        {
            var excelReader = new ExcelReader();
            var (testSteps, cycleGroups) = excelReader.ReadTestSteps(file);

            using var playwright = await Playwright.CreateAsync();

            IBrowserType browserType;

            if (browser == "chrome" || browser == "edge")
            {
                browserType = playwright.Chromium;
            }
            else if (browser == "firefox")
            {
                browserType = playwright.Firefox;
            }
            else
            {
                throw new ArgumentException($"{browser} is not a supported browser choice");
            }

            await using var browserInstance = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Channel = browser == "chrome" ? "chrome" :
                    browser == "edge" ? "msedge" : null
            });
            
            var executor = new TestExecutor(_broadcaster, testRunId);
            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            var reportHandler = new HandleReporting(_logger, _broadcaster, testRunId);

            await executor.ExecuteTestFileAsync(browserInstance, testSteps, env, fileName, cycleGroups, int.Parse(delay), _reportToDevops, reportHandler);
            
            // if (_reportToDevops)
            // {
            //     await reportHandler.ReportToDevOps(browserInstance, testSteps, env, fileName, Response, cycleGroups);
            // }
            // else
            // {
            //     await executor.ExecuteTestFileAsync(browserInstance, testSteps, env, fileName, cycleGroups, int.Parse(delay), _reportToDevops);
            // }
            //
            return Ok("Tests executed successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { Error = e.Message });
        }
    }
    
    //public IActionResult SaveTestSteps([FromForm] List<TestStep> testSteps)
    //{
    //    using (var workbook = new XLWorkbook())
    //    {
    //        var worksheet = workbook.Worksheets.Add("TestSteps");
    //        worksheet.Cell(1, 1).Value = "TestCaseName";
    //        worksheet.Cell(1, 2).Value = "TestDescription";

    //        for (int i = 0; i < testSteps.Count; i++)
    //        {
    //            worksheet.Cell(i + 2, 1).Value = testSteps[i].TestCaseName;
    //            worksheet.Cell(i + 2, 2).Value = testSteps[i].TestDescription;
    //        }

    //        using (var stream = new MemoryStream())
    //        {
    //            workbook.SaveAs(stream);
    //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Testing.xlsx");
    //        }
    //    }
    //}
}