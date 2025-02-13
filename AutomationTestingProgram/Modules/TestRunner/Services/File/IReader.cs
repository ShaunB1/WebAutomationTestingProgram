namespace AutomationTestingProgram.Modules.TestRunnerModule
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
        public TestRunObject TestRun { get; }

        /// <summary>
        /// Retrieves the current active test case
        /// as well as index of current step.
        /// </summary>
        (TestCaseObject TestCase, int TestStepIndex) GetNextTestStep();
    }
}
