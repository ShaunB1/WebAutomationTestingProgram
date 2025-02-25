using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;

namespace AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Executor
{   
    /// <summary>
    /// The interface used to define executor types.
    /// </summary>
    public interface IPlaywrightExecutor
    {   
        /// <summary>
        /// Executes a test file on the given page object.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task ExecuteTestFileAsync(Page page);

    }
}
