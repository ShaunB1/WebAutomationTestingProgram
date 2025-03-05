namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

public class TestStep
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
    public string CycleGroup { get; set; }
    public string Outcome { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public string Comment { get; set; }
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }
    public int SequenceIndex { get; set; }
    public bool RunSuccessful { get; set; }
    public string Actual { get; set; }

    public TestStep() { }
    public TestStep(TestStep other) {
        TestCaseName = other.TestCaseName;
        TestDescription = other.TestDescription;
        StepNum = other.StepNum;
        ActionOnObject = other.ActionOnObject;
        Object = other.Object;
        Value = other.Value;
        Comments = other.Comments;
        Release = other.Release;
        LocalAttempts = other.LocalAttempts;
        LocalTimeout = other.LocalTimeout;
        Control = other.Control;
        Collection = other.Collection;
        TestStepType = other.TestStepType;
        GoToStep = other.GoToStep;
        CycleGroup = other.CycleGroup;
        Outcome = other.Outcome;
        StartedDate = other.StartedDate;
        CompletedDate = other.CompletedDate;
        Comment = other.Comment;
        Actual = other.Actual;
        ErrorMessage = other.ErrorMessage;
        StackTrace = other.StackTrace;
        SequenceIndex = other.SequenceIndex;
        RunSuccessful = other.RunSuccessful;
    }
}