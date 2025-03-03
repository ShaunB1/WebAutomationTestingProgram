using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class CheckAllRadioButtons : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        try
        {
            var radioButtons = await page.QuerySelectorAllAsync("input[type='radio']");
            var selectedGroups = new HashSet<string>();

            foreach (var radioButton in radioButtons)
            {
                var isVisible = await radioButton.IsVisibleAsync();
                var isEnabled = await radioButton.IsEnabledAsync();

                if (isVisible && isEnabled)
                {
                    var groupName = await radioButton.GetAttributeAsync("name");

                    if (groupName == null || selectedGroups.Contains(groupName))
                    {
                        continue;
                    }
            
                    await radioButton.CheckAsync();
            
                    selectedGroups.Add(groupName);
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