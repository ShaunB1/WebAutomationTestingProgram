using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public interface IWebAction
{
    Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration);
}