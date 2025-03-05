using Microsoft.Playwright;

namespace WebAutomationTestingProgram.Actions;

public class RunJavaScript : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, Modules.TestRunnerV1.Models.TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams, Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration, string cycleGroupName)
    {
        var jsCmd = step.Value;
        
        try
        {
            await page.EvaluateAsync<string>(jsCmd);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to execute Javascript command: {ex.Message}");
        }
    }
}