using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class RefreshBrowser : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            await page.Instance.ReloadAsync();
            await page.LogInfo("Successfully refreshed page.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}