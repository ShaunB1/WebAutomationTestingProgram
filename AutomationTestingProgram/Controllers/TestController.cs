using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Backend.Managers;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Runtime.CompilerServices;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AzureDevOpsSettings _azureDevOpsSettings;
    private readonly HandleTestPlan _planHandler;
    private readonly HandleTestCase _caseHandler;
    private readonly bool _reportToDevops;
    private readonly ILogger<TestController> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly WebSocketLogBroadcaster _broadcaster;

    private static IBrowser _browser;
    private static readonly object BrowserLock = new object();
    private static ContextManager _contextManager;
    private static readonly object ContextLock = new object();

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings, ILogger<TestController> logger, ILoggerFactory loggerFactory, WebSocketLogBroadcaster broadcaster)
    {
        _azureDevOpsSettings = azureDevOpsSettings.Value;
        _planHandler = new HandleTestPlan();
        _caseHandler = new HandleTestCase();
        _reportToDevops = false;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _broadcaster = broadcaster;
    }

    private IBrowser Browser
    {
        get
        {
            if (_browser == null)
            {
                lock (BrowserLock)
                {
                    if (_browser == null)
                    {
                        var playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
                        _browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                        {
                            Headless = false,
                            Channel = "chrome"
                        }).GetAwaiter().GetResult();
                    }
                }
            }

            return _browser;
        }
    }

    private ContextManager Manager
    {
        get
        {
            if (_contextManager == null)
            {
                lock(ContextLock)
                {
                    if (_contextManager == null)
                    {
                        _contextManager = new ContextManager(Browser);
                    }
                }
            }

            return _contextManager;
        }
    }
    
    [HttpPost("run")] // IFormFile file
    public async Task<IActionResult> RunTests()
    {
        try
        {
            Manager.CreateNewContextAsync();
            return Ok("Tests execution started.");
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Error = e.Message });
        }

       
        Console.WriteLine("Received test request");
        Response.ContentType = "text/event-stream";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        var excelReader = new FileReader();
        var testSteps = excelReader.ReadTestSteps(file);
        
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Channel = "chrome"
        });
        
        var executor = new TestExecutor(_logger, _broadcaster);

        try
        {
            var environment = "EarlyON";
            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            var reportHandler = new HandleReporting(_logger, _broadcaster);

            if (_reportToDevops)
            {
                await reportHandler.ReportToDevOps(browser, testSteps, environment, fileName, Response);                
            }
            else
            {
                await executor.ExecuteTestCasesAsync(browser, testSteps, environment, fileName, Response);
            }
            
            return Ok("Tests executed successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { Error = e.Message });
        }
    }

    public IActionResult SaveTestSteps([FromForm] List<TestStep> testSteps)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("TestSteps");
            worksheet.Cell(1, 1).Value = "TestCaseName";
            worksheet.Cell(1, 2).Value = "TestDescription";

            for (int i = 0; i < testSteps.Count; i++)
            {
                worksheet.Cell(i+2, 1).Value = testSteps[i].TestCaseName;
                worksheet.Cell(i+2, 2).Value = testSteps[i].TestDescription;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Testing.xlsx");
            }
        }
    }
}