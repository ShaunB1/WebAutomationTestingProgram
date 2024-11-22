using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public class ExitCondition : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
    {
        try
        {
            var exitCondition = step.Value;
            var locator = step.Object;

            if (!string.IsNullOrEmpty(exitCondition))
            {
                if (exitCondition == "EXISTS")
                {
                    var element = page.Locator(locator);
                    var isVisible = await element.IsVisibleAsync();
                    if (isVisible)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return false;
    }
}