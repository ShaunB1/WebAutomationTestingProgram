using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class NavigateToURL : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        try
        {
            await page.GotoAsync(step.Value);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to navigate to url {step.Value}: {ex.Message}");
            return false;
        }
    }
}