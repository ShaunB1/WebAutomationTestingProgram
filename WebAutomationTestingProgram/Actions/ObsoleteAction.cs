using Microsoft.Playwright;

namespace WebAutomationTestingProgram.Actions;

public class ObsoleteAction : WebAction
{
    public override Task<bool> ExecuteAsync(IPage page, Modules.TestRunnerV1.Models.TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams, Dictionary<string, List<Dictionary<string, string>>> cycleGroups,
        int currentIteration, string cycleGroupName)
    {
        throw new NotImplementedException("Action is obsolete.");
    }
}