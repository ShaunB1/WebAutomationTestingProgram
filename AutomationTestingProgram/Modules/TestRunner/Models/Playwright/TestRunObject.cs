namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class TestRunObject
    {
        /// <summary>
        /// The name of the Test Run
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The id of the test run.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The id of the test plan the run is part of.
        /// </summary>
        public int PlanID { get; set; }

        /// <summary>
        /// The id of the test suite the run is part of.
        /// </summary>
        public int SuiteID { get; set; }


        /// <summary>
        /// The total # of Test Cases in the whole run.
        /// </summary>
        public int TestCaseNum => TestCases.Count;

        /// <summary>
        /// List of all TestCases within this TestRun
        /// </summary>
        public IList<TestCaseObject> TestCases { get; }


        /// <summary>
        /// The result of the TestRun
        /// </summary>
        public Result Result { get; set; } = Result.NotExecuted;

        /// <summary>
        /// The Start Date of the TestRun.
        /// </summary>
        public DateTime StartedDate { get; set; }

        /// <summary>
        /// The Completed Date of the TestRun
        /// </summary>
        public DateTime CompletedDate { get; set; }

        /// <summary>
        /// Total # of failed steps in Test Case
        /// </summary>
        public int FailureCounter { get; set; }



        public TestRunObject(string name)
        {
            Name = name;
            FailureCounter = 0;
            TestCases = new List<TestCaseObject>();
        }
    }

    public enum Result
    {
        /// <summary>
        /// Step uncomplete - still processing/yet to process. Neither failed nor successful.
        /// </summary>
        NotExecuted,

        /// <summary>
        /// Step failed
        /// </summary>
        Failed,

        /// <summary>
        /// Step successful
        /// </summary>
        Passed
    }
}
