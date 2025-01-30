using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class NavigateToURL : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        try
        {
            await pageObject.LogInfo($"Going to url {step.Value}");


            IPage page = pageObject.Instance!;

            var options = new PageGotoOptions
            {
                Timeout = 60000
            };

            await page.GotoAsync(step.Value, options);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to navigate to url {step.Value}: {ex.Message}");
        }
    }
}