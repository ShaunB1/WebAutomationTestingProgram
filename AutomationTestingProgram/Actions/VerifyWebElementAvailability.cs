using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class VerifyWebElementAvailability : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {

        IPage page = pageObject.Instance!;

        await pageObject.LogInfo("Locating element...");

        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);

        await pageObject.LogInfo("Element found.");

        var state = step.Value.ToLower();

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        switch (state)
        {
            case "exist":
                if (await element.IsVisibleAsync())
                {
                    await pageObject.LogInfo("Element exists.");
                    return;
                }
                break;
            case "does not exist":
                if (!(await element.IsVisibleAsync()))
                {
                    await pageObject.LogInfo("Element does exist");
                    return;
                }
                break;

            case "enabled":
                if (await element.IsEnabledAsync())
                {
                    await pageObject.LogInfo("Element enabled");
                    return;
                }

                break;
            case "disabled":
                if (await element.IsDisabledAsync() || await element.GetAttributeAsync("readonly") != null)
                {
                    await pageObject.LogInfo("Element disabled");
                    return;
                }

                break;
            default:
                 throw new Exception($"Unknown state: {state}");
        }

        throw new Exception($"Element is not in state: '{state}'");
    }
}