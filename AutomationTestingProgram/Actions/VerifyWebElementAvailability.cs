using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class VerifyWebElementAvailability : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var locator = step.Object;
        var state = step.Value.ToLower();
        var element = step.Comments == "html id" 
            ? page.Locator($"#{locator}") 
            : step.Comments == "innertext" 
                ? page.Locator($"text={locator}") 
                : page.Locator(locator);

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