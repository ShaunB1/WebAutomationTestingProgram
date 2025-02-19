using AutomationTestingProgram.Modules.TestRunnerModule;

using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class PressKey : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        var key = step.Value.ToLower();

        IPage page = pageObject.Instance!;

        if (key == "enter")
        {
            await page.Keyboard.PressAsync("Enter");
            await pageObject.LogInfo("Pressend ENTER");
        }
        else
        {
            await pageObject.LogInfo("Nothing pressed (only ENTER currently implemented)");
        }
    }
}