using Microsoft.Playwright;
using AutomationTestingProgram.ModelsOLD;

namespace AutomationTestingProgram.Backend.Actions;

public class PressKey : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
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