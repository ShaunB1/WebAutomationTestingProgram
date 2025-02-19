using AutomationTestingProgram.Modules.TestRunnerModule;

using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CloseWindow : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        await pageObject.LogInfo("Closing current Window...");

        await pageObject.CloseCurrentAsync();

        await pageObject.LogInfo("Successfully closed current Window.");

    }
}