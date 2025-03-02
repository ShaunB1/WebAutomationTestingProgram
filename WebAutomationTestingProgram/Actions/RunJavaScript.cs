using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class RunJavaScript : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var jsCmd = step.Value;
        
        try
        {
            await page.Instance.EvaluateAsync<string>(jsCmd);
            page.LogInfo("Javascript command executed successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to execute Javascript command: {ex.Message}");
        }
    }
}