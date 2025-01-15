using Microsoft.Playwright;
using AutomationTestingProgram.ModelsOLD;

namespace AutomationTestingProgram.Backend.Actions;

public interface IWebAction
{
    Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration);
}