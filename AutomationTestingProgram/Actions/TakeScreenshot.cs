using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class TakeScreenshot : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStepObject step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
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