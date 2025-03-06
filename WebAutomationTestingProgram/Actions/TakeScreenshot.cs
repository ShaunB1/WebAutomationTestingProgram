using Microsoft.Playwright;

namespace WebAutomationTestingProgram.Actions;

public class TakeScreenshot : WebAction
{
    public override Task<bool> ExecuteAsync(IPage page, Modules.TestRunnerV1.Models.TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams, Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration, string cycleGroupName)
    {
        // try
        // {
        //     var testCase = step.TestCaseName.Replace(" ", "").ToLower();
        //     var stepNum = step.StepNum;
        //     
        //     var path = page.RetrieveScreenShotFolder();
        //     var ssPath = Path.Combine(path, $"{testCase}(Step_{stepNum}).png)");
        //     
        //     await page.Instance.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(path, ssPath) });
        //
        //     page.LogInfo($"Successfully taken screenshot at {ssPath}");
        // }
        // catch (Exception ex)
        // {
        //     throw new Exception(ex.Message);
        // }
        throw new NotImplementedException();
    }
}