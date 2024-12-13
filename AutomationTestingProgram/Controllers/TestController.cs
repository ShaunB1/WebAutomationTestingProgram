using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Backend.Managers;
using AutomationTestingProgram.Models;
using AutomationTestingProgram.Models.Backend;
using AutomationTestingProgram.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Runtime.CompilerServices;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    //private readonly AzureDevOpsSettings _azureDevOpsSettings;
    //private readonly HandleTestPlan _planHandler;
    //private readonly HandleTestCase _caseHandler;
    //private readonly bool _reportToDevops;
    private readonly ILogger<TestController> _logger;
    private readonly ILoggerFactory _loggerFactory;
    //private readonly WebSocketLogBroadcaster _broadcaster;

    private static PlaywrightObject? _playwright = null;
    private static readonly object PlaywrightLock = new object();

    private static readonly ConcurrentDictionary<string, Request> ActiveRequests = new ConcurrentDictionary<string, Request>();

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings, ILogger<TestController> logger, ILoggerFactory loggerFactory, WebSocketLogBroadcaster broadcaster)
    {
        //_azureDevOpsSettings = azureDevOpsSettings.Value;
        //_planHandler = new HandleTestPlan();
        //_caseHandler = new HandleTestCase();
        //_reportToDevops = false;
        _logger = logger;
        _loggerFactory = loggerFactory;
        //_broadcaster = broadcaster;
    }

    /// <summary>
    /// Creates a Playwright Object to be used throughout the whole run. (ONLY ONE INSTANCE)
    /// </summary>
    private PlaywrightObject playwright
    {
        get
        {
            if (_playwright == null)
            {
                lock (PlaywrightLock)
                {
                    if (_playwright == null)
                    {
                        _playwright = new PlaywrightObject();
                    }
                }
            }

            return _playwright;
        }
    }

    /*
     * User permissions:
     * - When the user first logs in via AAD, their account information is stored via cookies/cache.
     * - We always know which user sent any request
     * - We also always know from where they sent any request
     * - We use the User to validate permissions
     * 
     * 1. Permission to use application (Keychain: Coarse Grain)
     * 2. Permission to use environment/organization (Keychain: Fine Grain)
     * 3. Are they an Admin or a User
     * 
     * Permissions must be verified, and feature testing with PEN testing
     */

    /* Log Folder System
     * 
     * Keep same structure
     * Add request folder under run.
     * All logs for a specific request go there.
     * However, ExecutionRequestLogs just show the file path to the context logs for
     * that request
     * 
     * 
     * 
     * 
     * 
     */


    /// <summary>
    /// Receives api requests to stop execution of another request
    /// Validation:
    ///     - Request to stop exists
    ///     - Request to stop is NOT a STOP request
    ///     - Valid permissions:
    ///         -> Permission to use application (Keychain: Coarse Grain)
    ///         -> Permission to use environment/organization of request to stop (Keychain: Fine Grain)
    ///         -> If Admin: Can then stop request
    ///         -> If User: Can only stop requests from self, not from other users
    /// </summary>
    /// <param name="ID">The ID of the request to stop.</param>
    /// <returns></returns>
    [HttpPost("stop")]
    public async Task<IActionResult> StopRequest(string ID)
    {
        
    }

    /// <summary>
    /// Receives api requests to validate files
    /// Validation:
    ///     - Valid permissions:
    ///         -> Permission to use application (Keychain: Coarse Grain)
    ///         -> Permission to use environment/organization (Keychain: Fine Grain)
    /// </summary>
    /// <param name="File">The File to validate</param>
    /// <returns></returns>
    public async Task<IActionResult> ValidateRequest(IFormFile File)
    {

    }
    
    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    /// <param name="File">The file to process</param>
    /// <param name="Env">The environment to run on</param>
    /// <returns></returns>
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest(IFormFile File, string Env)
    {
        // Create a request
        var request = new Request(ile, "chrome", 123);
        
        try
        {
            // Process the request and await a response.
            _logger.LogInformation($"Request (ID: {request.ID}) received.");
            ActiveRequests.TryAdd(request.ID, request);
            request = await playwright.ProcessRequestAsync(request);
            // If request succeeds
            _logger.LogInformation($"Request (ID: {request.ID}) successfully completed.");
            return Ok(new { Message = "Test Execution Complete.", Request = request});
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"Request (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
        finally
        {
            ActiveRequests.TryRemove(request.ID, out Request? var);
        }       
        
    }


        /*Console.WriteLine("Received test request");
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
            }*/
        public IActionResult SaveTestSteps([FromForm] List<TestStepV1> testSteps)
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