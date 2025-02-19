using AutomationTestingProgram.Modules.TestRunnerModule;


namespace AutomationTestingProgram.Actions;

public class ObsoleteAction : WebAction
{
    public override Task ExecuteAsync(Page page, string groupID, TestStepObject step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        throw new NotImplementedException("Action is obsolete.");
    }
}