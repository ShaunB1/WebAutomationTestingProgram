using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public class NavigateToURL : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
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