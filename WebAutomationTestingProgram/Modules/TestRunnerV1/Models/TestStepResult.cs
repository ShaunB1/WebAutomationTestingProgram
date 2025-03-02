using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

public class TestStepResult
{
    public string Action { get; set; }
    public string ExpectedResult { get; set; }
    public string ActualResult { get; set; }
    public TestOutcome Outcome { get; set; }
    public string Comment { get; set; }
}