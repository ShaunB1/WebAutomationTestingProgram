using AutomationTestingProgram.Models;
using AutomationTestingProgram.Actions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Diagnostics;
/*using AutoUpdater;*/
/*using AutomationTestingProgram.Helper*/

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
    private readonly WebSocketLogBroadcaster _broadcaster;
    private readonly IConfiguration _configuration;

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings, ILogger<TestController> logger, WebSocketLogBroadcaster broadcaster, IConfiguration configuration)
    {
        _azureDevOpsSettings = azureDevOpsSettings.Value;
        _planHandler = new HandleTestPlan();
        _caseHandler = new HandleTestCase();
        _reportToDevops = false;
        _logger = logger;
        _broadcaster = broadcaster;
        _configuration = configuration;
    }

    [HttpPost("test_post")]
    public IActionResult TestPost()
    {
        return Ok("POST request successful.");
    }

    [HttpGet("test_get")]
    public IActionResult TestGet()
    {
        return Ok("GET request successful.");
    }

    // We want to authorization for all endpoints, but if you are testing then comment out the line below
    [Authorize]
    [HttpPost("run")]
    public async Task<IActionResult> RunTests([FromForm] IFormFile file, [FromForm] string env, [FromForm] string browser, [FromForm] string browserVersion)
    {
        if (file == null)
        {
            return BadRequest("No file received.");
        }

        _logger.LogInformation("Received test request");
        // Response.ContentType = "text/event-stream";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
;
        try
        {
            _logger.LogInformation("Running Autoupdater");
            // Automatically run the AutoUpdater before starting the tests using helper 
            var updateResult = RunAutoUpdater(browser, browserVersion);
            if (!updateResult.IsSuccess)
            {
                return StatusCode(500, updateResult.ErrorMessage);
            }
            else
            {
                _logger.LogInformation($"{updateResult.ErrorMessage}");
            }

            var excelReader = new ExcelReader();
            var testSteps = excelReader.ReadTestSteps(file);

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

            var executor = new TestExecutor(_logger, _broadcaster);
            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            var reportHandler = new HandleReporting(_logger, _broadcaster);

            if (_reportToDevops)
            {
                await reportHandler.ReportToDevOps(browserInstance, testSteps, env, fileName, Response);
            }
            else
            {
                await executor.ExecuteTestCasesAsync(browserInstance, testSteps, env, fileName, Response);
            }

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

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Testing.xlsx");
            }
        }
    }

    // Helper method to run AutoUpdater
    private (bool IsSuccess, string ErrorMessage) RunAutoUpdater(string browser, string version)
    {
        try
        {
            if (string.IsNullOrEmpty(browser) || string.IsNullOrEmpty(version))
            {
                return (false, "Browser or version not provided.");
            }

            // Set the path to the AutoUpdater.exe
            var autoUpdaterPath = Path.Combine(Directory.GetCurrentDirectory(), "utils", "AutoUpdater", "AutoUpdater.exe");

            // Check if the AutoUpdater.exe exists
            if (!System.IO.File.Exists(autoUpdaterPath))
            {
                return (false, "AutoUpdater.exe not found in the root directory.");
            }

            // Set up the process start info
            var startInfo = new ProcessStartInfo
            {
                FileName = autoUpdaterPath,
                Arguments = $"--browser {browser} --version {version}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false, 
                CreateNoWindow = true 
            };

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    return (false, "Failed to start AutoUpdater process.");
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return (true, output); 
                }
                else
                {
                    return (false, error); 
                }
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message); 
        }
    }

    // New endpoint to get browser versions from appsettings
    [HttpGet("browserVersions")]
    public IActionResult GetBrowserVersions()
    {
        var browserVersions = _configuration.GetSection("AllowedBrowserVersions").Get<Dictionary<string, string>>();

        // Convert semicolon-separated versions into arrays
        var parsedVersions = new Dictionary<string, List<string>>();

        foreach (var browser in browserVersions)
        {
            parsedVersions[browser.Key] = new List<string>(browser.Value.Split(';'));
        }

        return Ok(parsedVersions);
    }
}