using AutomationTestingProgram.Services;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Models;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class TestExecutor
{
    private readonly int _testCaseId;
    private readonly Dictionary<string, IWebAction> _actions;
    private readonly bool _reportToDevops = false;
    private readonly bool _recordTrace = false;
    private readonly bool _recordVideo = false;
    private readonly ILogger<TestController> _logger;
    private readonly WebSocketLogBroadcaster _broadcaster;
    private Dictionary<string, string> _envVars = new Dictionary<string, string>();
    private Dictionary<string, string> _saveParameters = new Dictionary<string, string>();

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
            { "presskey", new PressKey() },
            { "selectddl", new SelectDDL() },
            { "runsqlscript", new RunSQLScript() },
            { "uploadfile", new UploadFile() },
            { "closewindow", new CloseWindow() },
            { "waitinseconds", new WaitInSeconds() },
            { "exitcondition", new ExitCondition() },
            { "chooseallddl", new ChooseAllDDL() },
            { "checkallradiobuttons", new CheckAllRadioButtons() },
            { "checkallboxes", new CheckAllBoxes() },
            { "fillalltextboxes", new FillAllTextBoxes() },
            { "verifytxtfile", new VerifyTxtFile() },
            { "comment", new Comment() },
            { "saveparameter", new SaveParameter() }
        };
    }

    /*public async Task ExecuteTestCasesAsync(IBrowser browser, List<TestStepV1> testSteps, string environment, string fileName, HttpResponse response)
    {
        // execute test steps
        _envVars["environment"] = environment;
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        //await page.SetViewportSizeAsync(1920, 1080);
        var testResults = new List<TestCaseResultParams>();
        var testCases = testSteps.Where(s => s.Control != "#").GroupBy(s => s.TestCaseName);
        var cycleGroups = testSteps.Where(s => s.Cycle != string.Empty).GroupBy(s => s.Cycle);
        var nonCycleGroups = testSteps.Where(s => s.Cycle == string.Empty).GroupBy(s => s.TestCaseName);

        var cycleDatasets = JsonConvert.DeserializeObject<List<List<string>>>(testCases.First().First().CycleData);
        var cycleIterations = cycleDatasets?.Count ?? -1;
        var cycleIteration = 0;

        foreach (var testCase in nonCycleGroups)
        {
            Console.WriteLine($"Executing Test Case: {testCase.Key}");
            var firstStep = testCase.First();
            var datasets = JsonConvert.DeserializeObject<List<List<string>>>(firstStep.Data);
            var iterations = datasets?.Count ?? -1;
            var iteration = 0;

            if (page.IsClosed)
            {
                await context.ClearCookiesAsync();
                page = await context.NewPageAsync();
                await page.SetViewportSizeAsync(1920, 1080);
            }

            if (firstStep.ActionOnObject.ToLower().Replace(" ", "") == "exitcondition")
            {
                if (_actions.TryGetValue(firstStep.ActionOnObject.ToLower().Replace(" ", ""), out var action))
                {
                    var res = await action.ExecuteAsync(page, firstStep, -1, _envVars, _saveParameters);
                    while (!res)
                    {
                        Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                        res = await action.ExecuteAsync(page, firstStep, -1, _envVars, _saveParameters);
                        Console.WriteLine($"EXIT CONDITION: {res}");

                        if (!res)
                        {
                            await ExecuteTestStepsAsync(page, testCase.ToList(), response, -1);
                        }
                    }
                }

                Console.WriteLine($"Moving to the next test case...");
                continue;
            }

            if (datasets == null)
            {
                await ExecuteTestStepsAsync(page, testCase.ToList(), response, iteration = -1);
            }

            while (iterations > 0)
            {
                Console.WriteLine($"Iteration: {iterations}");
                await ExecuteTestStepsAsync(page, testCase.ToList(), response, iteration);
                iterations -= 1;
                iteration += 1;
            }
        }

        foreach (var cycleGroup in cycleGroups)
        {
            while (cycleIterations > 0)
            {
                testCases = cycleGroup.GroupBy(s => s.TestCaseName);
                foreach (var testCase in testCases)
                {

                    Console.WriteLine($"Executing Test Case: {testCase.Key}");
                    var firstStep = testCase.First();
                    var datasets = JsonConvert.DeserializeObject<List<List<string>>>(firstStep.Data);
                    var iterations = datasets?.Count ?? -1;

                    var iteration = 0;


                    if (page.IsClosed)
                    {
                        await context.ClearCookiesAsync();
                        page = await context.NewPageAsync();
                        await page.SetViewportSizeAsync(1920, 1080);
                    }

                    if (firstStep.ActionOnObject.ToLower().Replace(" ", "") == "exitcondition")
                    {
                        if (_actions.TryGetValue(firstStep.ActionOnObject.ToLower().Replace(" ", ""), out var action))
                        {
                            var res = await action.ExecuteAsync(page, firstStep, -1, _envVars, _saveParameters);
                            while (!res)
                            {
                                Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                                res = await action.ExecuteAsync(page, firstStep, -1, _envVars, _saveParameters);
                                Console.WriteLine($"EXIT CONDITION: {res}");

                                if (!res)
                                {
                                    await ExecuteTestStepsAsync(page, testCase.ToList(), response, -1);
                                }
                            }
                        }

                        Console.WriteLine($"Moving to the next test case...");
                        continue;
                    }

                    if (datasets == null && cycleDatasets == null)
                    {
                        await ExecuteTestStepsAsync(page, testCase.ToList(), response, iteration = -1);
                    }

                    if (cycleDatasets != null)
                    {
                        await ExecuteTestStepsAsync(page, testCase.ToList(), response, cycleIteration);
                    }

                    while (iterations > 0)
                    {
                        Console.WriteLine($"Iteration: {iterations}");
                        await ExecuteTestStepsAsync(page, testCase.ToList(), response, iteration);
                        iterations -= 1;
                        iteration += 1;
                    }
                }

                cycleIterations -= 1;
                cycleIteration += 1;
            }
        }
    }

    public async Task<(string, List<(int, string)>, List<(int, string)>)> ExecuteTestStepsAsync(IPage page, List<TestStep> testSteps, HttpResponse response, int iteration, int maxAttempts = 2)
    {
        var stepsFailed = new List<(int, string)>();
        var failCount = 0;
        var stackTrace = new List<(int, string)>();
        var endLoop = false;

        foreach (var (step, index) in testSteps.Select((step, index) => (step, index)))
        {
            try
            {
                if (_actions.TryGetValue(step.ActionOnObject.ToLower().Replace(" ", ""), out var action))
                {
                    step.SequenceIndex = index + 1;
                    step.Comment = "{COMMENT}";
                    step.StartedDate = DateTime.UtcNow;

                    step.Object = InsertParams(step.Object);
                    step.Value = InsertParams(step.Value);

                    _logger.LogInformation($"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}, {step.Value}");
                    await _broadcaster.BroadcastLogAsync(
                        $"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}");

                    var success = await RetryActionAsync(async () =>
                    {
                        var stepDelay = 0;
                        var timeout = 30000;

                        if (int.TryParse(step.Control, out var delay))
                        {
                            stepDelay = delay;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(stepDelay));

                        if (step.LocalTimeout * 1000 > timeout)
                        {
                            timeout = step.LocalTimeout * 1000;
                        }

                        page.SetDefaultTimeout(timeout);

                        var res = await action.ExecuteAsync(page, step, iteration, _envVars, _saveParameters);

                        _logger.LogInformation($"TEST RESULT: {res}");
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
                            (index + 1, $"ACTION: [{step.ActionOnObject}] OBJECT: [{step.Object}], VALUE: [{step.Value}], COMMENT: [{step.Comments}]"));
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
    }*/

    // Replace parameters between "{" and "}" with the Saved Parameters. To be used with Object or Value string. 
    public string InsertParams(string input)
    {
        string pattern = @"\{([^}]+)\}";

        var matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            if (_saveParameters.ContainsKey(key))
            {
                input = input.Replace(match.Value, _saveParameters[key]);
            }
            else
            {
                Console.WriteLine($"Input parameter {key} does not exist in Save Parameters");
            }
        }
        return input;
    }
}
