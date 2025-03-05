using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Newtonsoft.Json;
using WebAutomationTestingProgram.Actions;
using WebAutomationTestingProgram.Core.Helpers;
using WebAutomationTestingProgram.Core.Hubs;
using WebAutomationTestingProgram.Core.Hubs.Services;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Services.AzureReporter;
using Exception = System.Exception;

namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Services;

public class TestExecutor
{
    private readonly int _testCaseId;
    private readonly string _testRunId;
    private readonly Dictionary<string, WebAction> _actions;
    private readonly bool _reportToDevops = false;
    private readonly bool _recordTrace = false;
    private readonly bool _recordVideo = false;
    private Dictionary<string, string> _envVars = new Dictionary<string, string>();
    private Dictionary<string, string> _saveParameters = new Dictionary<string, string>();
    private readonly SignalRService _signalRService;
    private List<int> _testCaseIds = new List<int>();
    private int _testPlanId;
    private int _testSuiteId;
    private int _devopsRunId;

    private static Dictionary<string, string> actionAliases = new Dictionary<string, string>
        {
            // Populate actions
            { "populatetextbox", "populatewebelement" },
            { "populatehtmleditor", "populatewebelement" },
            { "populateframe", "populatewebelement" },
            { "populatetextboxddl", "populatewebelement" },

            // Verify state actions
            { "verifylinkavailability", "verifywebelementavailability" },
            { "verifybuttonavailability", "verifywebelementavailability" },
            { "verifytextboxavailability", "verifywebelementavailability" },
            { "verifywebradiogroupavailability", "verifywebelementavailability" },
            { "verifycheckboxstatus", "verifywebelementavailability" },
            { "verifycheckboxavailablity", "verifywebelementavailability" },
            { "verifyddlavailability", "verifywebelementavailability" },
            { "verifyhtmleditoravailability", "verifywebelementavailability" },
            { "verifyimageavailability", "verifywebelementavailability" },
            { "verifywebtableavailability", "verifywebelementavailability" },
            
            // Verify content actions
            { "verifyddlcontent", "verifywebelementcontent" },
            { "verifyhtmleditorcontent", "verifywebelementcontent" },
            { "verifyimagecontent", "verifywebelementcontent" },
            { "verifytextboxcontent", "verifywebelementcontent" },

            // Click actions
            { "clickbutton", "clickwebelement" },
            { "clicklink", "clickwebelement" },
            { "clickimage", "clickwebelement" },
            { "clicktablelink", "clickwebelement" },
            { "selectlookup", "clickwebelement" },
            { "selectwebradiogroup", "clickwebelement" },

            // Login actions
            { "enteraadcredentials", "login" },

            // SQL actions
            { "runprsqlscriptrevert", "runsqlscript" },
            { "runprsqlscriptdelete", "runsqlscript" },

            // Upload actions
            { "uploaddatafile", "uploadfile" },
            
            // Other
            { "gotopage", "navigatetourl" },
        };

    public TestExecutor(SignalRService signalRService, string testRunId)
    {
        _signalRService = signalRService;
        _testRunId = testRunId;
        
        var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Core\\StaticFiles\\Public\\actions.json");
        var jsonContent = File.ReadAllText(jsonFilePath);
        var actionTypes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

        _actions = actionTypes.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                Type actionType = Type.GetType(kvp.Value);
                if (actionType == null)
                {
                    throw new InvalidOperationException($"Action type '{kvp.Value}' not found.");
                }
                return (WebAction)Activator.CreateInstance(actionType);
            }
        );
    }

    public async Task ExecuteTestFileAsync(IBrowser browser,
        List<TestStep> testSteps,
        string environment,
        string fileName,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int delay, bool reportToDevops,
        HandleReporting reportHandler)
    {
        _envVars["environment"] = environment;
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var iterationStack = new Stack<int>();
        var currentIteration = 0;
        
        await ExecuteNestedLoopsAsync(page, testSteps, cycleGroups, iterationStack, currentIteration, delay, context, reportToDevops, reportHandler);
    }

    public async Task ExecuteNestedLoopsAsync(IPage page, List<TestStep> steps,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, Stack<int> iterationStack,
        int currentIteration, int delay, IBrowserContext context, bool reportToDevops, HandleReporting reportHandler)
    {
        try
        {
            
            var stepsCopy = new List<TestStep>(steps);
            for (var i = 0; i < stepsCopy.Count; i++)
            {
                if (page.IsClosed)
                {
                    await context.ClearCookiesAsync();
                    await context.AddInitScriptAsync("window.localStorage.clear(); window.sessionStorage.clear();");
                    page = await context.NewPageAsync();
                }
                
                await Task.Delay(delay * 1000);
                
                var step = stepsCopy[i];
                var parts = step.CycleGroup.Split(",");
                var cycleGroup = parts.Length > 1 ? parts[0].Trim() : string.Empty;
                var state = parts.Length > 1 ? parts[1].Trim().ToLower() : string.Empty;
                var iterations = -1;

                if (cycleGroups.TryGetValue(cycleGroup, out var it))
                {
                    iterations = it.Count;
                }
                    
                List<TestStep> loopSteps = new List<TestStep>();
                
                if (state == "start")
                {
                    loopSteps = GetLoopSteps(stepsCopy);
                    for (var j = 0; j < iterations; j++)
                    {
                        foreach (var loopStep in loopSteps)
                        {
                            var stepAction = loopStep.ActionOnObject.Replace(" ", "").ToLower();
                            var actionAlias = GetAlias(stepAction);

                            if (page.IsClosed)
                            {
                                await context.ClearCookiesAsync();
                                await context.AddInitScriptAsync("window.localStorage.clear(); window.sessionStorage.clear();");
                                page = await context.NewPageAsync();
                            }
                            
                            var logMessage = LogMessageHelper.GenerateTestExecutionLog(loopStep);
                            await _signalRService.BroadcastLog(_testRunId, logMessage);
                            
                            if (_actions.TryGetValue(actionAlias, out var action))
                            {
                                var result = await action.ExecuteAsync(page, loopStep, _envVars, _saveParameters, cycleGroups, j, cycleGroup);
                                loopStep.RunSuccessful = result;

                                var statusMessage = LogMessageHelper.GenerateTestStepStatusLog(loopStep);

                                await _signalRService.BroadcastLog(_testRunId, statusMessage);
                            }
                            else
                            {
                                loopStep.RunSuccessful = false;
                                var statusMessage = LogMessageHelper.GenerateTestStepStatusLog(loopStep);

                                await _signalRService.BroadcastLog(_testRunId, statusMessage);
                                throw new Exception($"Unknown action '{loopStep.ActionOnObject}'");
                            }
                            
                            await Task.Delay(TimeSpan.FromSeconds(delay));
                        }
                    }

                    var startIndex = stepsCopy.IndexOf(loopSteps.First());
                    var endIndex = stepsCopy.IndexOf(loopSteps.Last());
                    stepsCopy = stepsCopy.Take(startIndex).Concat(stepsCopy.Skip(endIndex + 1)).ToList();
                    i = startIndex - 1;
                }
                else
                {
                    var stepAction = step.ActionOnObject.Replace(" ", "").ToLower();
                    var actionAlias = GetAlias(stepAction);
                    var logMessage = LogMessageHelper.GenerateTestExecutionLog(step);

                    await _signalRService.BroadcastLog(_testRunId, logMessage);
                    
                    if (_actions.TryGetValue(actionAlias, out var action))
                    { 
                        var result = await action.ExecuteAsync(page, step, _envVars, _saveParameters, cycleGroups, currentIteration, cycleGroup);
                        step.RunSuccessful = result;

                        var statusMessage = LogMessageHelper.GenerateTestStepStatusLog(step);

                        await _signalRService.BroadcastLog(_testRunId, statusMessage);
                    }
                    else
                    {
                        step.RunSuccessful = false;
                        var statusMessage = LogMessageHelper.GenerateTestStepStatusLog(step);

                        await _signalRService.BroadcastLog(_testRunId, statusMessage);
                        throw new Exception($"Unknown action '{step.ActionOnObject}'");
                    }
                }
            }

            var testResults = new List<TestCaseResultParams>();
            var testCases = steps
                .GroupBy(s => s.TestCaseName)
                .Select(g => new TestCase
                {
                    TestCaseName = g.Key,
                    TestSteps = g.ToList(),
                    Result = g.Any(step => !step.RunSuccessful) ? "Failed" : "Passed"
                })
                .ToList();
            
            if (reportToDevops)
            {
                await reportHandler.DeleteTestCasesAsync();
                await reportHandler.DeleteTestPlanAsync();
                (_testCaseIds, _testPlanId, _testSuiteId) = await reportHandler.InitializeTestPlanAsync(steps);
                _devopsRunId = await reportHandler.CreateTestRunAsync();
            }
            
            foreach (var (testCase, index) in testCases.Select((tc, index) => (tc, index)))
            {
                try
                {
                    var testResultObj = new TestCaseResultParams();
                    testResultObj.testCaseId = _testCaseIds[index];
                    testResultObj.testCaseName = testCase.TestCaseName;
                    testResultObj.startedDate = DateTime.UtcNow;

                    var testPointHandler = new HandleTestPoint();
                    var testPoint =
                        await testPointHandler.GetTestPointFromTestCaseIdAsync(_testPlanId, _testSuiteId,
                            _testCaseIds[index]);

                    testResultObj.testPointId = testPoint.Id;
                    testResultObj.completedDate = DateTime.UtcNow;
                    testResultObj.outcome = testCase.Result;
                    testResultObj.state = "Completed";
                    testResultObj.duration =
                        Convert.ToInt32((testResultObj.completedDate - testResultObj.startedDate).TotalMilliseconds);
                    // testResultObj.errorMsg = testResultObj.outcome == "Failed" ? $"[{failedTests.Count()} STEPS FAILED]\n{string.Join("\n", failedTests.Select(ft => $" {ft.Item1}: {ft.Item2}"))}" : null;
                    // testResultObj.stackTrace = testResultObj.outcome == "Failed" ? $"{string.Join("\n", stackTrace.Select(st => $"{st.Item1}: {st.Item2}"))}" : null;

                    testResults.Add(testResultObj);
                    
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            var testResultHandler = new HandleTestResult();
            await testResultHandler.UpdateTestResultsAsync(testResults, _devopsRunId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public string GetAlias(string action)
    {
        if (actionAliases.TryGetValue(action, out var alias))
        {
            return alias;
        }

        return action;
    }
    
    public List<TestStep> GetLoopSteps(List<TestStep> steps)
    {
        var startIndex = -1;
        var endIndex = -1;

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var parts = step.CycleGroup.Split(",");
            var state = parts.Length > 1 ? parts[1].Trim().ToLower() : string.Empty;

            if (state == "start")
            {
                if (startIndex == -1)
                {
                    startIndex = i;
                }
            }
            else if (state == "end")
            {
                endIndex = i;
                break;
            }
        }

        if (startIndex == -1 || endIndex == -1 || startIndex > endIndex)
        {
            throw new InvalidOperationException("Invalid start or stop state.");
        }
        
        return steps.GetRange(startIndex, endIndex - startIndex + 1);
    }
    
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
    
        public async Task<(string, List<(int, string)>, List<(int, string)>)> ExecuteTestStepsAsync(IPage page,
        List<TestStep> testSteps,
        HttpResponse response,
        int iteration,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int maxAttempts = 1)
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
                    
                    // await _signalRService.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}");

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

                        // var res = await action.ExecuteAsync(page, step, iteration, _envVars, _saveParameters, cycleGroups, TODO);

                        // _logger.LogInformation($"TEST RESULT: {res}");
                        // await _broadcaster.BroadcastLogAsync($"TEST RESULT: {res}", _testRunId);
                        return false; // return res
                    }, maxAttempts);

                    step.CompletedDate = DateTime.UtcNow;

                    if (!success)
                    {
                        // _logger.LogInformation($"FAILED: {step.Object}");
                        // await _signalRService.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"FAILED: {step.Object}");
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
                await _signalRService.BroadcastLog(_testRunId, e.ToString());
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
                    await _signalRService.BroadcastLog(_testRunId, $"Attempt {attempt} failed with error: {e.Message}");
                    await Task.Delay(1000);
                }
            }

            return false;
        }
    }
}

class TestCase
{
    public string TestCaseName { get; set; }
    public List<TestStep> TestSteps { get; set; }
    public string Result { get; set; }
}