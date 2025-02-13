using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;

namespace AutomationTestingProgram.Actions;

public class NotImplementedAction : WebAction
{
    public override Task ExecuteAsync(Page page, string groupID, TestStepObject step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        throw new NotImplementedException("Action not implemented.");
    }
}