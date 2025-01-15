using Microsoft.Playwright;
using AutomationTestingProgram.ModelsOLD;


namespace AutomationTestingProgram.Backend.Actions;

public class CloseWindow : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
    {
        await page.CloseAsync();

        return true;
    }
}