using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class RefreshBrowser : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, Modules.TestRunnerV1.Models.TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams, Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration, string cycleGroupName)
    {
        try
        {
            await page.ReloadAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}