namespace AutomationTestingProgram.Modules.TestRunnerModule;

/// <summary>
/// Represents a TestStep for automated tests.
/// </summary>
public class TestStep(
string testCaseName,
string testDescription,
int stepNum,
string actionOnObject,
string objectName,
string value,
string comments,
string release,
int localAttempts,
int localTimeout,
string control,
string collection,
int testStepType,
string goToStep)
{   
    /// <summary>
    /// Name of assocaited Test Case
    /// </summary>
    public string TestCaseName { get; } = testCaseName;

    /// <summary>
    /// The description of the TestStep
    /// </summary>
    public string TestDescription { get; } = testDescription;

    /// <summary>
    /// The Step # of the Test Step.
    /// Property is being depreciated, but kept both for backwards compatability
    /// and GOTOSTEP. Note: Will be removed entirely once GoToStep is refactored
    /// to not need StepNum.
    /// </summary>
    public int StepNum { get; } = stepNum;

    /// <summary>
    /// The ActionOnObject this Step uses.
    /// </summary>
    public string ActionOnObject { get; } = actionOnObject;

    /// <summary>
    /// Assocaited information on the object column (information depends on ActionOnObject)
    /// Most often useCases:
    /// - Locator(s) provided to find the element
    /// </summary>
    public string Object { get; set; } = objectName;

    /// <summary>
    /// Associated information on the value column (information depends on ActionOnObject)
    /// Most often useCases:
    /// - Text used to populate an element, like a TextArea
    /// - Text used to validate element found contains said text
    /// </summary>
    public string Value { get; set; } = value;

    /// <summary>
    /// Associated information on the comments column (information depends on ActionOnObject)
    /// Most often useCases:
    /// - Identifies the type of locator found in Object field
    /// </summary>
    public string Comments { get; } = comments;

    /// <summary>
    /// The release # of the collection the test is made for.
    /// Property is being depreciated, but kept for backwards compatibility.
    /// to not need StepNum.
    /// </summary>
    public string Release { get; } = release;

    /// <summary>
    /// The # of attempts a step should make before being marked as a fail.
    /// Default: 3 attempts
    /// </summary>
    public int LocalAttempts { get; set; } = localAttempts;

    /// <summary>
    /// The timeout used by a step determining how long to wait for a certain Action to complete.
    /// Ex: Waiting a max of x seconds until we say -> element not found
    /// Default: 30 seconds
    /// </summary>
    public int LocalTimeout { get; set; } = localTimeout;

    /// <summary>
    /// Associated information on the control column (information depends on ActionOnObject)
    /// Most often useCases:
    /// - Comments: Type # in Control, and the step will be ignored
    /// </summary>
    public string Control { get; } = control;

    /// <summary>
    /// The collection the test is made for.
    /// Property is being depreciated, but kept for backwards compatibility.
    /// to not need StepNum.
    /// </summary>
    public string Collection { get; } = collection;

    /// <summary>
    /// The Type of TestStep. Default: 3
    /// 1 -> MANDATORY. If step fails, whole test execution ends in a faulted state.
    /// 2 -> IMPORTANT. If step fails, updates failure counter and percentage.
    ///                 Failure counter/percentage reset PER TEST CASE.
    ///                 If a TestCase has either:
    ///                    - counter >= 5 
    ///                         -> Each failed type 2 adds one to counter
    ///                    - percentage >= 33% (from total) 
    ///                         -> Each failed type 2 increases percentage by:
    ///                            1 / TestCase.TotalNumOfSteps (Ex: If 5 total steps, + 0.2)
    ///                 Whole test execution ends in a faulted state.
    /// 3 -> OPTIONAL. If step fails, marked as pass with error message.
    /// 4 -> GOTOSTEP. Field GoToStep must be filled in format: x,y
    ///                If step passes, goes to Test Step with Step # x, in SAME TEST CASE.
    ///                If step fails, goes to Test Steo with Step # y, in SAME TEST CASE.
    ///                NOTE: This will be changed in the future
    ///                Can Only Loop Max: 10 times
    /// 5 -> INVERTED. If step fails, marked as pass.
    ///                If step passes, marked as fail.
    ///                Follows same failure as 2.
    ///
    /// </summary>
    public int TestStepType { get; set; } = testStepType;

    /// <summary>
    /// Field GoToStep must be filled in format: x,y
    /// x -> If step passes
    /// y -> If step fails.
    /// Note: Mark TestStepType as type 4 to use.
    /// </summary>
    public string GoToStep { get; } = goToStep;

    // public string CycleGroup { get; } // Ingored for now


    /// <summary>
    /// The result of the TestStep
    /// </summary>
    public Result Result { get; set; }

    /// <summary>
    /// The Start Date of the TestStep
    /// </summary>
    public DateTime StartedDate { get; set; }

    /// <summary>
    /// The Completed Date of the TestStep
    /// </summary>
    public DateTime CompletedDate { get; set; }

    /// <summary>
    /// Total # of failed attempts
    /// </summary>
    public int FailureCounter { get; set; } = 0;
}