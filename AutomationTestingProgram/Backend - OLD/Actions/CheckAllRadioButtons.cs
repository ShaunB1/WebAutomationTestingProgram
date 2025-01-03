using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckAllRadioButtons : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
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