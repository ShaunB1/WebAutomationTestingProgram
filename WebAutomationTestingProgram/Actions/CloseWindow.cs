using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class CloseWindow : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        await page.CloseAsync();
        return true;
    }
}