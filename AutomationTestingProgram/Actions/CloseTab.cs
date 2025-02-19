using AutomationTestingProgram.Modules.TestRunnerModule;


namespace AutomationTestingProgram.Actions;

public class CloseTab : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStepObject step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            await page.CloseCurrentAsync();
            await page.LogInfo("Successfully closed tab.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}