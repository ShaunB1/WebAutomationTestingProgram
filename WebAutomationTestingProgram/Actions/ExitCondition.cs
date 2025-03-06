using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class ExitCondition : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        try
        {
            GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
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