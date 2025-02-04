using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace AutomationTestingProgram.Actions;

public class ChooseAllDDL : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        try
        {
            IPage page = pageObject.Instance!;

            await pageObject.LogInfo("Locating all DDL...");
            var selectElements = await page.QuerySelectorAllAsync("select");

            await pageObject.LogInfo($"DDLs successfully located: {selectElements.Count}");

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

            await pageObject.LogInfo("First option selected in all DDLs");
        }
        catch (Exception)
        {
            throw;
        }
    }
}