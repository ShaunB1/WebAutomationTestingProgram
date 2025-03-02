using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class ExitCondition : WebAction
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
                        return;
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}