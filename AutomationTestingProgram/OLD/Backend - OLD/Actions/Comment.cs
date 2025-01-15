using AutomationTestingProgram.OLD.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;
public class Comment : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        return true;
    }
}