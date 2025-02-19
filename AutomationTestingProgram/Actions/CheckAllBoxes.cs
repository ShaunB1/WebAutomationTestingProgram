using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckAllBoxes : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        try
        {
            IPage page = pageObject.Instance!;

            await pageObject.LogInfo("Locating all checkboxes...");

            var checkboxes = await page.QuerySelectorAllAsync("input[type='checkbox']");

            await pageObject.LogInfo($"Checkboxes successfully located: {checkboxes.Count}");

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

            await pageObject.LogInfo("All checkboxes are checked");
        }
        catch (Exception)
        {
            throw;
        }
    }
}