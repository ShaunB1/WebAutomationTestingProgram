using Microsoft.Playwright;
using AutomationTestingProgram.ModelsOLD;


namespace AutomationTestingProgram.Backend.Actions;

public class NavigateToURL : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
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