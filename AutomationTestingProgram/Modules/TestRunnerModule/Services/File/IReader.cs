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
        public TestRun TestRun { get; }

        /// <summary>
        /// Retrieves the current active test case
        /// </summary>
        TestCase GetCurrentTestCase();

        /// <summary>
        /// Retrieves the Next Test Step.
        /// </summary>
        /// <returns></returns>
        Task<TestStep> GetTestStepAsync();
    }
}
