using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public interface IWebAction
{
    Task<bool> ExecuteAsync(IPage page, TestStep step);
}