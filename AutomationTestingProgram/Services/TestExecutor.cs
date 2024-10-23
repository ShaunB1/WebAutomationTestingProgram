using System.ComponentModel;
using AutomationTestingProgram.Services;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.TestManagement.WebApi;

public class TestExecutor
{
    private readonly int _testCaseId;
    private readonly Dictionary<string, IWebAction> _actions;
    private readonly bool _reportToDevops = false;
    private readonly bool _recordTrace = false;
    private readonly bool _recordVideo = false;
    private readonly ILogger<TestController> _logger;
    private readonly WebSocketLogBroadcaster _broadcaster;

    public TestExecutor(ILogger<TestController> logger, WebSocketLogBroadcaster broadcaster)
    {
        _logger = logger;
        _broadcaster = broadcaster;
        _actions = new Dictionary<string, IWebAction>
        {
            { "clickwebelement", new ClickWebElement() },
            { "populatewebelement", new PopulateWebElement() },
            { "navigatetourl", new NavigateToURL() },
            { "verifywebelementavailability", new VerifyWebElementAvailability() },
            { "checkbox", new CheckBox() },
            { "login", new Login() },
            { "presskey", new PresKey() },
            { "selectddl", new SelectDDL() },
        };
        
    }

    public async Task ExecuteTestCasesAsync(IBrowser browser, List<TestStep> testSteps, string environment, string fileName, HttpResponse response)
    {
        var testCases = testSteps.GroupBy(s => s.TestCaseName);
        
        // execute test steps
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var testResults = new List<TestCaseResultParams>();
        
        foreach (var (testCase, index) in testCases.Select((tc, index) => (tc, index)))
        {
            var (testCaseResult, failedTests, stackTrace) = await ExecuteTestStepsAsync(page, testCase.ToList(), response);
        }
    }
    
    public async Task<(string, List<(int, string)>, List<(int, string)>)> ExecuteTestStepsAsync(IPage page, List<TestStep> testSteps, HttpResponse response, int maxAttempts = 2)
    {
        var stepsFailed = new List<(int, string)>();
        var failCount = 0;
        var stackTrace = new List<(int, string)>();

        foreach (var (step, index) in testSteps.Select((step, index) => (step, index)))
        {
            try
            {
                if (_actions.TryGetValue(step.ActionOnObject.ToLower().Replace(" ", ""), out var action))
                {
                    step.SequenceIndex = index+1;
                    step.Comment = "{COMMENT}";
                    step.StartedDate = DateTime.UtcNow;
                
                    // _logger.LogInformation($"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}");
                    await _broadcaster.BroadcastLogAsync(
                        $"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}");
                
                    bool success = await RetryActionAsync(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(step.LocalTimeout));
                        var res = await action.ExecuteAsync(page, step);
                        // _logger.LogInformation($"TEST RESULT: {res}");
                        await _broadcaster.BroadcastLogAsync($"TEST RESULT: {res}");
                        return res;
                    }, maxAttempts);
                
                    step.CompletedDate = DateTime.UtcNow;
                
                    if (!success)
                    {
                        // _logger.LogInformation($"FAILED: {step.Object}");
                        await _broadcaster.BroadcastLogAsync($"FAILED: {step.Object}");
                        step.Outcome = "Failed";
                        stepsFailed.Add((step.SequenceIndex, step.TestDescription));
                        failCount++;
                        stackTrace.Add(
                            (index+1, $"ACTION: [{step.ActionOnObject}] OBJECT: [{step.Object}], VALUE: [{step.Value}], COMMENT: [{step.Comments}]"));
                    }
                    else
                    {
                        step.Outcome = "Passed";
                    }
                };
            }
            catch (Exception e)
            {
                // _logger.LogInformation(e.ToString());
                await _broadcaster.BroadcastLogAsync(e.ToString());
                var indexedStackTrace = new List<(int, string)>();
                
                // return ("Failed", stepsFailed, new List<(int, string)>(index+1, e.StackTrace?.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)));
            }
            
        }
        return failCount == 0 ? ("Passed", stepsFailed, stackTrace) : ("Failed", stepsFailed, stackTrace);
        async Task<bool> RetryActionAsync(Func<Task<bool>> action, int maxAttempts)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (await action())
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    // _logger.LogInformation($"Attempt {attempt} failed with error: {e.Message}");
                    await _broadcaster.BroadcastLogAsync($"Attempt {attempt} failed with error: {e.Message}");
                    await Task.Delay(1000);
                }
            }

            return false;
        }
    }
}
