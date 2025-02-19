using AutomationTestingProgram.Modules.TestRunnerModule;


namespace AutomationTestingProgram.Actions;

public class NotImplementedAction : WebAction
{
    public override Task ExecuteAsync(Page page, string groupID, TestStepObject step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        throw new NotImplementedException("Action not implemented.");
    }
}