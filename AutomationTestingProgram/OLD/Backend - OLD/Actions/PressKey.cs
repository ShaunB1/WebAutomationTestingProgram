using AutomationTestingProgram.OLD.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class PressKey : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var key = step.Value.ToLower();

        if (key == "enter")
        {
            await page.Keyboard.PressAsync("Enter");
            return true;
        }

        return false;
    }
}