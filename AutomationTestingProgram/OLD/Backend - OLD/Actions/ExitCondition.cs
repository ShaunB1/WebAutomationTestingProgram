using AutomationTestingProgram.OLD.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class ExitCondition : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
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