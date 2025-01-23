using System.Text;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Models;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class TestExecutor
{
    private readonly int _testCaseId;
    private readonly string _testRunId;
    private readonly Dictionary<string, WebAction> _actions;
    private readonly bool _reportToDevops = false;
    private readonly bool _recordTrace = false;
    private readonly bool _recordVideo = false;
    private readonly ILogger<TestController> _logger;
    private readonly WebSocketLogBroadcaster _broadcaster;
    private Dictionary<string, string> _envVars = new Dictionary<string, string>();
    private Dictionary<string, string> _saveParameters = new Dictionary<string, string>();

    public TestExecutor(ILogger<TestController> logger, WebSocketLogBroadcaster broadcaster, string testRunId)
    {
        _logger = logger;
        _broadcaster = broadcaster;
        _testRunId = testRunId;
        
        var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StaticFiles\\actions.json");
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

    public async Task ExecuteTestFileAsync(IBrowser browser, List<TestStep> testSteps, string environment, string fileName, Dictionary<string, List<Dictionary<string, string>>> cycleGroups)
    {
        _envVars["environment"] = environment;
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var iterationStack = new Stack<int>();
        var currentIteration = 0;
        
        await ExecuteNestedLoopsAsync(page, testSteps, cycleGroups, iterationStack, currentIteration);
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

                    _logger.LogInformation($"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}, {step.Value}");
                    await _broadcaster.BroadcastLogAsync(
                        $"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}", _testRunId);

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
                        await _broadcaster.BroadcastLogAsync($"FAILED: {step.Object}", _testRunId);
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
                await _broadcaster.BroadcastLogAsync(e.ToString(), _testRunId);
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
                    await _broadcaster.BroadcastLogAsync($"Attempt {attempt} failed with error: {e.Message}", _testRunId);
                    await Task.Delay(1000);
                }
            }

            return false;
        }
    }

    public async Task ExecuteNestedLoopsAsync(IPage page, List<TestStep> steps,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, Stack<int> iterationStack,
        int currentIteration)
    {
        try
        {
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
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
                    loopSteps = GetLoopSteps(steps);

                    for (var j = 0; j < iterations; j++)
                    {
                        foreach (var loopStep in loopSteps)
                        {
                            if (_actions.TryGetValue(loopStep.ActionOnObject.Replace(" ", "").ToLower(), out var action))
                            {
                                var logMessage = new StringBuilder()
                                    .AppendLine("========================================================")
                                    .AppendLine("                TEST EXECUTION LOG                      ")
                                    .AppendLine("========================================================")
                                    .AppendLine($"TEST CASE:     {step.TestCaseName,-40}")
                                    .AppendLine($"DESCRIPTION:   {step.TestDescription,-40}")
                                    .AppendLine("--------------------------------------------------------")
                                    .AppendLine($"ACTION:        {step.ActionOnObject,-40}")
                                    .AppendLine($"OBJECT:        {step.Object,-40}")
                                    .AppendLine($"VALUE:         {step.Value,-40}")
                                    .AppendLine("--------------------------------------------------------")
                                    .AppendLine($"EXECUTING...")
                                    .AppendLine("========================================================")
                                    .ToString();
                                _logger.LogInformation($"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}, {step.Value}");
                                
                                await _broadcaster.BroadcastLogAsync(logMessage, _testRunId);
                                
                                var result = await action.ExecuteAsync(page, step, _envVars, _saveParameters, cycleGroups, currentIteration, cycleGroup);
                                step.RunSuccessful = result;
                                
                                _logger.LogInformation($"STATUS: {step.RunSuccessful}");

                                var statusMessage = new StringBuilder()
                                    .AppendLine($"STATUS: {step.RunSuccessful}")
                                    .AppendLine()
                                    .ToString();
                                
                                await _broadcaster.BroadcastLogAsync(statusMessage, _testRunId);
                            }
                        }
                    }

                    var startIndex = steps.IndexOf(loopSteps.First());
                    var endIndex = steps.IndexOf(loopSteps.Last());
                    steps = steps.Take(startIndex).Concat(steps.Skip(endIndex + 1)).ToList();
                    i = startIndex - 1;
                }
                else
                {
                    if (_actions.TryGetValue(step.ActionOnObject.Replace(" ", "").ToLower(), out var action))
                    { 
                        var logMessage = new StringBuilder()
                            .AppendLine("========================================================")
                            .AppendLine("                TEST EXECUTION LOG                      ")
                            .AppendLine("========================================================")
                            .AppendLine($"TEST CASE:     {step.TestCaseName,-40}")
                            .AppendLine($"DESCRIPTION:   {step.TestDescription,-40}")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine($"ACTION:        {step.ActionOnObject,-40}")
                            .AppendLine($"OBJECT:        {step.Object,-40}")
                            .AppendLine($"VALUE:         {step.Value,-40}")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine($"EXECUTING...")
                            .AppendLine("========================================================")
                            .ToString();
                        _logger.LogInformation($"ACTION: {step.TestCaseName}, {step.StepNum}, {step.TestDescription}, {step.ActionOnObject}, {step.Object}, {step.Value}");
                        
                        await _broadcaster.BroadcastLogAsync(logMessage, _testRunId);
                        
                        var result = await action.ExecuteAsync(page, step, _envVars, _saveParameters, cycleGroups, currentIteration, cycleGroup);
                        step.RunSuccessful = result;
                        
                        _logger.LogInformation($"STATUS: {step.RunSuccessful}");

                        var statusMessage = new StringBuilder()
                            .AppendLine($"STATUS: {step.RunSuccessful}")
                            .AppendLine()
                            .ToString();
                        
                        await _broadcaster.BroadcastLogAsync(statusMessage, _testRunId);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
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
        
        return steps.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
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
}
