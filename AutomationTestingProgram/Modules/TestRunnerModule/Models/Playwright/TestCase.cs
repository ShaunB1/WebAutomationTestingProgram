namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Represents a TestCase for automated tests.
    /// </summary>
    public class TestCase : IComparable<TestCase>
    {   
        /// <summary>
        /// The TestRun this case is part of.
        /// </summary>
        public TestRun TestRun { get; }
        
        /// <summary>
        /// The name of the TestCase that is part of the run.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The starting row position of the testCase
        /// in the provided file.
        /// </summary>
        public int RowPos { get; }

        /// <summary>
        /// The total # of steps in the whole TestCase.
        /// Can be increased with looping (dynamic) -> Only Cycles
        /// </summary>
        public int TestStepNum { get; set; }

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


        public TestCase(TestRun run, string name, int rowPos)
        {
            TestRun = run;
            Name = name;
            RowPos = rowPos;
            TestStepNum = 0;
            FailureCounter = 0;
            TestSteps = new List<TestStep>();
        }

        public int CompareTo(TestCase other)
        {
            if (other == null)
                return 1; // Greater if other null

            // TestRun.Name
            int runNameComparison = string.Compare(this.TestRun.Name, other.TestRun.Name, StringComparison.Ordinal);
            if (runNameComparison != 0)
                return runNameComparison;

            // Name
            int nameComparison = string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0)
                return nameComparison;

            // TestStepNum
            return this.TestStepNum.CompareTo(other.TestStepNum);
        }

        public override bool Equals(object obj)
        {
            if (obj is TestCase other)
            {
                return this.CompareTo(other) == 0;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TestRun.Name, Name, TestStepNum);
        }
    }
}
