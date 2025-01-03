using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public TestController(ILogger<TestController> logger)
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
        try
        {
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) received.");

            await RequestHandler.ProcessAsync(request);

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
    }


    /// <summary>
    /// Receives api requests to retrieve all active requests.
    /// </summary>
    [Authorize]
    [HttpPost("retrieve")]
    public async Task<IActionResult> GetActiveRequests() // Maybe add options for different types of retrievals??
    {
        RetrievalRequest request = new RetrievalRequest(HttpContext.User);
        return await HandleRequest<RetrievalRequest>(request);        
    }


    /// <summary>
    /// Receives api requests to stop execution of another request
    /// </summary>
    [Authorize]
    [HttpPost("stop")]
    public async Task<IActionResult> StopRequest([FromBody] CancellationRequestModel model)
    {
        CancellationRequest request = new CancellationRequest(HttpContext.User, model.ID);
        return await HandleRequest<CancellationRequest>(request);
    }

    /// <summary>
    /// Receives api requests to validate files
    /// </summary>
    [Authorize]
    [HttpPost("validate")] 
    public async Task<IActionResult> ValidateRequest([FromForm] ValidationRequestModel model)
    {
        ValidationRequest request = new ValidationRequest(HttpContext.User, model.File);
        return await HandleRequest<ValidationRequest>(request);
    }

    /// <summary>
    /// Receives api requests to process files
    /// </summary>
    [Authorize]
    [HttpPost("run")] 
    public async Task<IActionResult> RunRequest([FromForm] ProcessRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ProcessRequest request = new ProcessRequest(HttpContext.User, model.File, model.Type, model.Version, model.Environment);
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
            }
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
    }*/
}