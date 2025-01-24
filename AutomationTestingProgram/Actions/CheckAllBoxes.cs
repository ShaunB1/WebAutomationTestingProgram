using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckAllBoxes : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        try
        {
            var checkboxes = await page.QuerySelectorAllAsync("input[type='checkbox']");

            foreach (var checkbox in checkboxes)
            {
                var isVisible = await checkbox.IsVisibleAsync();
                var isEnabled = await checkbox.IsEnabledAsync();

                if (isVisible && isEnabled)
                {
                    var isChecked = await checkbox.IsCheckedAsync();

                    if (!isChecked)
                    {
                        await checkbox.CheckAsync();
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