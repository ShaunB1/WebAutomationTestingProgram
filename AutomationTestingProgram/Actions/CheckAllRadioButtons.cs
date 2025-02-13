using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckAllRadioButtons : WebAction
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

            await pageObject.LogInfo("Locating all radiobuttons...");

            var radioButtons = await page.QuerySelectorAllAsync("input[type='radio']");
            var selectedGroups = new HashSet<string>();

            await pageObject.LogInfo($"Radiobuttons successfully located: {radioButtons.Count}");

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

            await pageObject.LogInfo($"One radiobutton in each group is checked. Total # of Groups: {selectedGroups.Count}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}