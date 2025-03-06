using Microsoft.Playwright;

namespace WebAutomationTestingProgram.Actions;

public class CloseTab : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, Modules.TestRunnerV1.Models.TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams, Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration, string cycleGroupName)
    {
        try
        {
            GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
            await page.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}