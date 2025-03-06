using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class ChooseAllDDL : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        try
        {
            var selectElements = await page.QuerySelectorAllAsync("select");
            GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
            foreach (var selectElement in selectElements)
            {
                var isVisible = await selectElement.IsVisibleAsync();
                var isEnabled = await selectElement.IsEnabledAsync();
                
                if (isVisible && isEnabled)
                {
                    var selectValue = await selectElement.InputValueAsync();
                    if (string.IsNullOrEmpty(selectValue))
                    {
                        var options = await selectElement.QuerySelectorAllAsync("option");
                
                        if (options.Count > 0)
                        {
                            var firstOption = await options[0].GetAttributeAsync("value");
                            if (firstOption != null)
                            {
                                await selectElement.SelectOptionAsync(firstOption);
                            }
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}