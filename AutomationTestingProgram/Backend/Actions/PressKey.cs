using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public class PressKey : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
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