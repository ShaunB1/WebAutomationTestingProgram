using System.Diagnostics;
using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class WaitInSeconds : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        var delay = step.Value;

        if (int.TryParse(delay, out var delayInt))
        {
            await pageObject.LogInfo($"Waiting for {delay} seconds");
            var stopwatch = Stopwatch.StartNew();
            await Task.Delay(TimeSpan.FromSeconds(delayInt));
            stopwatch.Stop();
        }

        return;
    }
}