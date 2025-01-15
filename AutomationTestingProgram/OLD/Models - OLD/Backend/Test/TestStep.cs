namespace AutomationTestingProgram.ModelsOLD
{
    public class TestStep
    {

        public int ID { get; set; }
        public string Name { get; }
        public TestCase TestCase { get; set; }
        public bool LastTestStep { get; set; }
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

        public TestStep(string Name)
        {
            this.Name = Name;
            this.Result = Result.NONE;
            this.StartedDate = DateTime.Now;
        }
    }
}
