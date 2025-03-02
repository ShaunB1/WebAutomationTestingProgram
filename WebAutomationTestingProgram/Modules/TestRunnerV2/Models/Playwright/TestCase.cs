namespace WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright
{
    /// <summary>
    /// Represents a TestCase for automated tests.
    /// </summary>
    public class TestCase
    {   
        /// <summary>
        /// The name of the TestCase that is part of the run.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The total # of steps in the whole TestCase.
        /// </summary>
        public int TestStepNum => TestSteps.Count;

        /// <summary>
        /// List of all TestSteps within this TestCase
        /// </summary>
        public IList<TestStep> TestSteps { get; }


        /// <summary>
        /// The result of the TestCase
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// The Start Date of the TestCase
        /// </summary>
        public DateTime StartedDate { get; set; }

        /// <summary>
        /// The Completed Date of the TestCase
        /// </summary>
        public DateTime CompletedDate { get; set; }

        /// <summary>
        /// Total # of failed steps in Test Case
        /// </summary>
        public int FailureCounter { get; set; }


        public TestCase(string name)
        {
            Name = name;
            FailureCounter = 0;
            TestSteps = new List<TestStep>();
        }
    }
}
