namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class TestRun
    {
        /// <summary>
        /// The name of the Test Run
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The total # of Test Cases in the whole run
        /// Can be increased with looping (dynamic) -> Only cycles
        /// </summary>
        public int TestCaseNum { get; set; }


        /// <summary>
        /// The result of the TestRun
        /// </summary>
        public Result Result { get; set; }

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



        public TestRun(string name)
        {
            Name = name;
            TestCaseNum = 0;
            FailureCounter = 0;
        }
    }

    public enum Result
    {
        /// <summary>
        /// Step uncomplete - still processing/yet to process. Neither failed nor successful.
        /// </summary>
        Uncomplete,

        /// <summary>
        /// Step failed
        /// </summary>
        Failed,

        /// <summary>
        /// Step successful
        /// </summary>
        Successful
    }
}
