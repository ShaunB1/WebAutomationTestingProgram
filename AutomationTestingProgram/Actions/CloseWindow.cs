using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CloseWindow : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
    {
        await page.CloseAsync();
        
        return true;
    }
}