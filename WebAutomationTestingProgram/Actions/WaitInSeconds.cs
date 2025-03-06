using System.Diagnostics;
using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class WaitInSeconds : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        var delay = step.Value;

        if (int.TryParse(delay, out var delayInt))
        {
            var stopwatch = Stopwatch.StartNew();
            await Task.Delay(TimeSpan.FromSeconds(delayInt));
            stopwatch.Stop();
            Console.WriteLine($"DELAY EXPECTED: {delayInt}, DELAY ACTUAL: {stopwatch.Elapsed}");
        }
        
        return true;
    }
}