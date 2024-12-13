using System.Diagnostics;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public class WaitInSeconds : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
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