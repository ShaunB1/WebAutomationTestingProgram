using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class VerifyWebElementAvailability : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);
        
        var state = step.Value.ToLower();

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        switch (state)
        {
            case "exist":
                if (await element.IsVisibleAsync())
                {
                    return true;
                }

                break;
            case "does not exist":
                if (!(await element.IsVisibleAsync()))
                {
                    return true;
                }

                break;
            case "enabled":
                if (await element.IsEnabledAsync())
                {
                    return true;
                }

                break;
            case "disabled":
                if (await element.IsDisabledAsync() || await element.GetAttributeAsync("readonly") != null)
                {
                    return true;
                }

                break;
            default:
                Console.WriteLine($"Unknown state: {state}");
                return false;
        }

        return false;
    }
}