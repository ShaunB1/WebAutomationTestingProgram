using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Backend.Request;
using AutomationTestingProgram.Models;


// using AutomationTestingProgram.Backend.Managers;
using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Runtime.CompilerServices;

/* Add a dictionary of objects -> hold all requets by id
 * Add a semaphore to have a limit of requests. If exceed limit, throw
 * Create Validator class. Similar to BrowserManager, has queue, and active.
 * Will validate Permissions, files, etc.
 * 
 * Once Validator is finished, update Execute functions for all requests.
 * 
 * Then, update BrowserManager, downwards
 * 
 * THEN, websocket connections
 * Last -> DevOps
 * 
 * 
 */

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

    private static readonly ConcurrentDictionary<string, object> ActiveRequests = new ConcurrentDictionary<string, object>();
    private static readonly SemaphoreSlim MaxRequests = new SemaphoreSlim(150); // Up to 150 requests queued or active at a time.
    private static readonly object ShutDownLock = new object();
    private static bool isShuttingDown = false;

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

    /* API Request Examples:
     * - STOP
     * curl -X POST -H "Content-Type: application/json" -d "{\"ID\": \"request-id-to-stop\"}" http://localhost:5223/api/test/stop
     * - VALIDATE
     *   curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" http://localhost:5223/api/test/validate
     * - PROCESS
     * curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" -F "Type=Chrome" -F "Version=92" -F "Environment=TestEnv" http://localhost:5223/api/test/run
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
    public async Task<IActionResult> StopRequest([FromBody] CancellationRequestModel model)
    {

        lock (ShutDownLock)
        {
            if (isShuttingDown)
            {
                _logger.LogError($"ShutDown already in progress. Request ignored.");
                return StatusCode(503, new { Error = "ShutDown already in progress. Request ignored.", Request = (object?)null });
            }
        }

        if (model == null || string.IsNullOrEmpty(model.ID))
        {
            return BadRequest(new { Error = "ID is required." }); 
        }
        
        if (!MaxRequests.Wait(0))
        {
            _logger.LogError($"Request received but ignored. Too many requests active. Please try again later.");
            return StatusCode(503, new { Error = "Too many requests. Please try again later.", Request = (object?)null });
        }

        // Create a CANCELLATION REQUEST
        CancellationRequest request = new CancellationRequest(model.ID);

        try
        {
            // Process the request and await a response.
            _logger.LogInformation($"Cancellation Request (ID: {request.ID}) received.");
            ActiveRequests.TryAdd(request.ID, request);

            await request.Execute();

            // If request succeeds
            _logger.LogInformation($"Cancellation Request (ID: {request.ID}) successfully completed.");
            return Ok(new { Message = $"Cancellation Request (ID: {request.ID}) Complete.", Request = request });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"Cancellation Request (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
        finally
        {
            ActiveRequests.TryRemove(request.ID, out _);
            MaxRequests.Release();
        }
    }

    /// <summary>
    /// Receives api requests to shut down application
    /// Validation:
    ///     - Permissions -> MUST BE SUPER USER
    /// </summary>
    /// <param name="force">Whether its a force shutdown (stops everything)</param>
    /// <returns></returns>
    [HttpPost("shutdown")]
    public async Task<IActionResult> ShutDown([FromBody] bool force = false)
    {
        // SHUTDOWN request always pre-dominates other requests. If multiple received, rest are ignored.

        lock (ShutDownLock)
        {
            if (isShuttingDown)
            {
                _logger.LogError($"ShutDown already in progress. Request ignored.");
                return StatusCode(503, new { Error = "ShutDown already in progress. Request ignored.", Request = (object?)null });
            }
            else
            {
                isShuttingDown = true;
            }
        }

        // Create a CANCELLATION REQUEST
        ShutDownRequest request = new ShutDownRequest(force);

        try
        {
            // Process the request and await a response.
            _logger.LogInformation($"ShutDown Request (ID: {request.ID}) received. {(force ? "FORCED SHUTDOWN" : "GRACEFUL SHUTDOWN")}");
            await request.Execute();

            // No need for logs, application will be terminated in request. Just here to keep away errors.
            return Ok(new { Message = $"ShutDown Request (ID: {request.ID}) Complete.", Request = request });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"ShutDownRequest Request (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
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
    [HttpPost("validate")] 
    public async Task<IActionResult> ValidateRequest([FromForm] ValidationRequestModel model)
    {
        lock (ShutDownLock)
        {
            if (isShuttingDown)
            {
                _logger.LogError($"ShutDown already in progress. Request ignored.");
                return StatusCode(503, new { Error = "ShutDown already in progress. Request ignored.", Request = (object?)null });
            }
        }

        if (model == null || model.File == null || model.File.Length == 0)
        {
            return BadRequest(new { Error = "No File Uploaded." });
        }

        if (!MaxRequests.Wait(0))
        {
            _logger.LogError($"Request received but ignored. Too many requests active. Please try again later.");
            return StatusCode(503, new { Error = "Too many requests. Please try again later.", Request = (object?)null });
        }

        // Create a CANCELLATION REQUEST
        ValidationRequest request = new ValidationRequest(model.File);

        try
        {
            // Process the request and await a response.
            _logger.LogInformation($"Validation Request (ID: {request.ID}) received.");
            ActiveRequests.TryAdd(request.ID, request);

            await request.Execute();

            // If request succeeds
            _logger.LogInformation($"Validation Request (ID: {request.ID}) successfully completed.");
            return Ok(new { Message = $"Validation Request (ID: {request.ID}) Complete.", Request = request });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"Validation Request (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
        finally
        {
            ActiveRequests.TryRemove(request.ID, out _);
            MaxRequests.Release();
        }
    }
    
    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    /// <param name="File">The file to process</param>
    /// <param name="Type">The Type of Browser to run on</param>
    /// <param name="Version">The Version of Browser to run on</param>
    /// <param name="Env">The environment to run on</param>
    /// <returns></returns>
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest([FromForm] ProcessRequestModel model)
    {
        lock (ShutDownLock)
        {
            if (isShuttingDown)
            {
                _logger.LogError($"ShutDown already in progress. Request ignored.");
                return StatusCode(503, new { Error = "ShutDown already in progress. Request ignored.", Request = (object?)null });
            }
        }

        if (model == null || model.File == null || model.File.Length == 0)
        {
            return BadRequest(new { Error = "No File Uploaded." });
        }

        if (string.IsNullOrEmpty(model.Type) || string.IsNullOrEmpty(model.Version) || string.IsNullOrEmpty(model.Environment))
        {
            return BadRequest(new { Error = "Environment, Browser Type and Version must be provided!" });
        }

        if (!MaxRequests.Wait(0))
        {
            _logger.LogError($"Request received but ignored. Too many requests active. Please try again later.");
            return StatusCode(503, new { Error = "Too many requests. Please try again later.", Request = (object?)null });
        }

        // Create a CANCELLATION REQUEST
        ProcessRequest request = new ProcessRequest(model.File, model.Type, model.Version, model.Environment);

        try
        {
            // Process the request and await a response.
            _logger.LogInformation($"Process Request (ID: {request.ID}) received.");
            ActiveRequests.TryAdd(request.ID, request);

            await request.Execute();

            // If request succeeds
            _logger.LogInformation($"Process Request (ID: {request.ID}) successfully completed.");
            return Ok(new { Message = $"Process Request (ID: {request.ID}) Complete.", Request = request });
        }
        catch (Exception e)
        {
            // If request fails
            _logger.LogError($"Process Request (ID: {request.ID}) failed.\nError: '{e.Message}'");
            return StatusCode(500, new { Error = e.Message, Request = request });
        }
        finally
        {
            ActiveRequests.TryRemove(request.ID, out _);
            MaxRequests.Release();
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