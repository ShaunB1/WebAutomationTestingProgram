using WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Playwright;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Services.File
{
    public interface IReader
    {
        /// <summary>
        /// Gets whether the ExcelReader finished reading all steps.
        /// </summary>
        public bool isComplete { get; }

        /// <summary>
        /// The run associated with this excel reader.
        /// </summary>
        public TestRun TestRun { get; }

        /// <summary>
        /// Retrieves the current active test case
        /// as well as index of current step.
        /// </summary>
        (TestCase TestCase, int TestStepIndex) GetNextTestStep();
    }
}
