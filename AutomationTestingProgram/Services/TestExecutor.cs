using AutomationTestingProgram.Services;
using AutomationTestingProgram.Actions;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Playwright;

public class TestExecutor
{
    private readonly IBrowser _browser;
    private readonly AzureDevOpsReporter _reporter;
    private readonly int _testCaseId;
    private readonly Dictionary<string, IWebAction> _actions;
    private readonly bool _reportToDevops = false;
    private readonly bool _recordTrace = false;
    private readonly bool _recordVideo = false;

    public TestExecutor(IBrowser browser, AzureDevOpsReporter reporter)
    {
        _browser = browser;
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
        _reporter = reporter;
    }

    public async Task ExecuteTestCasesAsync(List<TestStep> testSteps, string environment, string fileName)
    {
        var fileSuiteId = _reportToDevops ? await _reporter.AzureDevOpsReporterInit(environment, testSteps.FirstOrDefault(), fileName) : -1;
        var testCases = testSteps.GroupBy(s => s.TestCaseName);
        var testRun = _reportToDevops ? await _reporter.CreateTestRunAsync($"Test Run") : null;
        var tasks = new List<Task>();
        var testCaseIds = new List<int>();
        var context1 = _recordVideo ? await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            RecordVideoDir = @"C:\Users\BautisSh\Downloads\",
            RecordVideoSize = new RecordVideoSize
            {
                Width = 1280,
                Height = 720
            }
        }) : await _browser.NewContextAsync();
        
        if (_recordTrace)
        {
            await context1.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }
        
        await context1.ClearCookiesAsync();
        var page1 = await context1.NewPageAsync();

        if (_reportToDevops)
        {
            foreach (var testCase in testCases)
            {
                var testCaseId = await _reporter.CreateTestCaseAsync(testCase.Key, "");
                testCaseIds.Add(testCaseId);
                tasks.Add(_reporter.AddTestCaseToSuiteAsync(fileSuiteId, testCaseId));
            }
        
            await Task.WhenAll(tasks);
            tasks.Clear();

            foreach (var (testCase, index) in testCases.Select((tc, idx) => (tc, idx)))
            {
                var steps = testCase.Select(step => (step.TestDescription, $"LOCATOR_TYPE: {step.Comments} OBJECT: {step.Object}, VALUE: {step.Value}")).ToList();
                tasks.Add(_reporter.AddTestStepsToTestCaseAsync(testCaseIds[index], steps));
            }
        
            await Task.WhenAll(tasks);
            tasks.Clear();
        }

        foreach (var (testCase, index) in testCases.Select((tc, idx) => (tc, idx)))
        {
            var testCaseResult = await ExecuteTestStepsAsync(testCase.ToList()) ? "Passed" : "Failed";
            if (_reportToDevops)
            {
                await _reporter.RecordTestCaseResultAsync(fileSuiteId, testCase.Key, testCaseIds[index], testCaseResult);
            }
            Console.WriteLine($"Test Case '{testCase.Key}' {testCaseResult}");
        }
        
        Console.WriteLine($"Test run completed.");

        if (_recordTrace)
        {
            await context1.Tracing.StopAsync(new TracingStopOptions
            {
                Path = @"C:\Users\BautisSh\Downloads\trace.zip",
            });
        }
        
        await page1.CloseAsync();
        await context1.CloseAsync();
        await _browser.CloseAsync();
        
        await _browser.CloseAsync();
        
        async Task<bool> ExecuteTestStepsAsync(List<TestStep> testSteps, int maxAttempts = 2)
        {
            var failCount = 0;

            foreach (var (step, index) in testSteps.Select((step, index) => (step, index)))
            {
                if (_actions.TryGetValue(step.ActionOnObject.ToLower().Replace(" ", ""), out var action))
                {
                    step.SequenceIndex = index;
                    step.Comment = "{COMMENT}";
                    step.StartedDate = DateTime.UtcNow;
                    Console.WriteLine($"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {action}, {step.Object}");
                    bool success = await RetryActionAsync(async () =>
                    {
                        await page1.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        await page1.WaitForTimeoutAsync(step.LocalTimeout * 1000);
                        var res1 = await action.ExecuteAsync(page1, step);
                        Console.WriteLine($"TEST RESULT: {res1}");
                        return true;
                    }, maxAttempts);
                    
                    step.CompletedDate = DateTime.UtcNow;
                    
                    if (!success)
                    {
                        Console.WriteLine($"FAILED: {step.Object}");
                        step.Outcome = "Failed";
                        failCount++;
                    }
                    else
                    {
                        step.Outcome = "Passed";
                    }
                };
            }

            return failCount == 0;
            
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
                        Console.WriteLine($"Attempt {attempt} failed with error: {e.Message}");
                        await Task.Delay(1000);
                    }
                }

                return false;
            }
        }
    }
}