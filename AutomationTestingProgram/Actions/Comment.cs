using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;
public class Comment : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        return;
    }
}