using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class TakeScreenshot : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            var testCase = step.TestCaseName.Replace(" ", "").ToLower();
            var stepNum = step.StepNum;
            
            var path = page.RetrieveScreenShotFolder();
            var ssPath = Path.Combine(path, $"{testCase}(Step_{stepNum}).png)");
            
            await page.Instance.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(path, ssPath) });

            page.LogInfo($"Successfully taken screenshot at {ssPath}");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}