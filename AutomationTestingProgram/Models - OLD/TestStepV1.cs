public class TestStepV1
{
    public string TestCaseName { get; set; }
    public string TestDescription { get; set; }
    public int StepNum { get; set; }
    public string ActionOnObject { get; set; }
    public string Object { get; set; }
    public string Value { get; set; }
    public string Comments { get; set; }
    public string Release { get; set; }
    public int LocalAttempts { get; set; }
    public int LocalTimeout { get; set; }
    public string Control  { get; set; }
    public string Collection  { get; set; }
    public string TestStepType { get; set; }
    public int GoToStep { get; set; }
    public string Data { get; set; }
    public string Cycle { get; set; }
    public string CycleData { get; set; }

    public string Outcome { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public string Comment { get; set; }
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }
    public int SequenceIndex { get; set; }

    public bool RunSuccessful { get; set; }

    public string Actual { get; set; }
}