using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class PresKey : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step)
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