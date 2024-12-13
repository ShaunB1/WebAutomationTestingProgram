namespace AutomationTestingProgram.Models.Backend
{   
    /// <summary>
    /// Represents a series of Test Steps
    /// </summary>
    public class TestCase
    {
        public int ID { get; set; }
        public string Name { get; }
        public List<TestStep> TestSteps { get; }
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

        public TestCase(string Name)
        {
            this.Name = Name;
            this.TestSteps = new List<TestStep>();
            this.Result = Result.NONE;
            this.StartedDate = DateTime.Now;
        }


    }
}
