using Microsoft.Playwright;

namespace AutomationTestingProgram.OLD.Actions;

public interface IWebAction
{
    Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams);
}