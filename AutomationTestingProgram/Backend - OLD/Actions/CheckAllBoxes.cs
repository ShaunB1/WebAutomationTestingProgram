using Microsoft.Playwright;
using AutomationTestingProgram.ModelsOLD;

namespace AutomationTestingProgram.Backend.Actions;

public class CheckAllBoxes : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
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