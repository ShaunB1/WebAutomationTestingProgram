using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public interface IWebAction
{
    Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration);
}