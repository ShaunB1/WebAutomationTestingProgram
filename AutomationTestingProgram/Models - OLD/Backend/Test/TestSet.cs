namespace AutomationTestingProgram.ModelsOLD
{   

    /// <summary>
    /// Test Set represents a whole file. Contains all test cases.
    /// Note: Cycles create different test cases with different iteration #
    /// </summary>
    public class TestSet
    {
        public int ID { get; set;  }
        public string Name { get; }
        public List<TestCase> TestCases { get; }
        public Result Result { get; set; }
        public DateTime StartedDate { get; }
        public DateTime? CompletedDate { get; }
        public int duration
        {
            get
            {
                if (!CompletedDate.HasValue)
                    return -1;
                return Convert.ToInt32((CompletedDate.Value - StartedDate).TotalMilliseconds);
            }
        }
        public string errMsg { get; set; }
        public string stackTrace { get; set; }

        public TestSet(string Name)
        {
            this.Name = Name;
            this.TestCases = new List<TestCase>();
            this.Result = Result.NONE;
            this.StartedDate = DateTime.Now;
        }
    }

    public enum Result
    {
        NONE,

        PASS,

        FAIL
    }
}
