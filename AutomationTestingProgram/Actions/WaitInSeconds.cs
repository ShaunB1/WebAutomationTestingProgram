using System.Diagnostics;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class WaitInSeconds : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
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