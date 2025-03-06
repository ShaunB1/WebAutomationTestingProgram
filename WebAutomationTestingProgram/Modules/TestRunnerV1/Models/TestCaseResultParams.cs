using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

public class TestCaseResultParams
{
    public int testCaseId { get; set; }
    public string testCaseName { get; set; }
    public string outcome { get; set; }
    public string state { get; set; }
    public System.DateTime startedDate { get; set; }
    public System.DateTime completedDate { get; set; }
    public int duration { get; set; }
    public string errorMsg { get; set; }
    public string stackTrace { get; set; }
    public int testPointId { get; set; }
    public int testCaseRevision { get; set; }
    public TestIterationDetailsModel testIterationDetails { get; set; }
}