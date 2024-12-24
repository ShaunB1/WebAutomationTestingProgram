using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Models;


// using AutomationTestingProgram.Backend.Managers;
using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Org.BouncyCastle.Asn1.Ocsp;
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
    //private readonly WebSocketLogBroadcaster _broadcaster;

    public TestController(IOptions<AzureDevOpsSettings> azureDevOpsSettings, ILogger<TestController> logger, WebSocketLogBroadcaster broadcaster)
    {
        //_azureDevOpsSettings = azureDevOpsSettings.Value;
        //_planHandler = new HandleTestPlan();
        //_caseHandler = new HandleTestCase();
        //_reportToDevops = false;
        _logger = logger;
        //_broadcaster = broadcaster;
    }

    /* API Request Examples:
     * - STOP
     * curl -X POST -H "Content-Type: application/json" -d "{\"ID\": \"request-id-to-stop\"}" http://localhost:5223/api/test/stop
     * - VALIDATE
     *   curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" http://localhost:5223/api/test/validate
     * - PROCESS
     * curl -X POST -H "Content-Type: multipart/form-data" -F "File=@C:\\Users\\DobrinD\\Downloads\\Schools Table.xlsx" -F "Type=Chrome" -F "Version=92" -F "Environment=TestEnv" http://localhost:5223/api/test/run
     * - GETACTIVEREQUESTS
     * curl -X POST -H "Content-Type: application/json" http://localhost:5223/api/test/retrieve
     * 
     * Test commands:
     * for /l %i in (1,1,10) do start /b curl -X POST -H "Content-Type: application/json" http://localhost:5223/api/test/retrieve
     */

    private async Task<IActionResult> HandleRequest<TRequest>(TRequest request) where TRequest : IClientRequest
    {
        if (!RequestHandler.TryAcquireRequestSlot(request))
        {
            _logger.LogError($"Too many requests active. Please try again later.");
            return StatusCode(503, new { Error = "Too many requests. Please try again later.", Request = (object?)null });
        }

        try
        {
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) received.");

            _ = RequestHandler.ProcessRequestAsync(request);
            await request.ResponseSource.Task;

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
        finally
        {
            RequestHandler.ReleaseRequestSlot(request);
        }
    }


    /// <summary>
    /// Receives api requests to retrieve all active requests.
    /// </summary>
    [HttpPost("retrieve")]
    public async Task<IActionResult> GetActiveRequests() // Maybe add options for different types of retrievals??
    {
        RetrievalRequest request = new RetrievalRequest();
        return await HandleRequest<RetrievalRequest>(request);        
    }


    /// <summary>
    /// Receives api requests to stop execution of another request
    /// </summary>
    [HttpPost("stop")]
    public async Task<IActionResult> StopRequest([FromBody] CancellationRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        CancellationRequest request = new CancellationRequest(model.ID);
        return await HandleRequest<CancellationRequest>(request);
    }

    /// <summary>
    /// Receives api requests to validate files
    /// </summary>
    [HttpPost("validate")] 
    public async Task<IActionResult> ValidateRequest([FromForm] ValidationRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ValidationRequest request = new ValidationRequest(model.File);
        return await HandleRequest<ValidationRequest>(request);
    }
    
    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest([FromForm] ProcessRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ProcessRequest request = new ProcessRequest(model.File, model.Type, model.Version, model.Environment);
        return await HandleRequest<ProcessRequest>(request);
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