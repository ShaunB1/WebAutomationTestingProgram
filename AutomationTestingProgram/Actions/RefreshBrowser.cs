using AutomationTestingProgram.Modules.TestRunnerModule;


namespace AutomationTestingProgram.Actions;

public class RefreshBrowser : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStepObject step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            await page.Instance.ReloadAsync();
            await page.LogInfo("Successfully refreshed page.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}