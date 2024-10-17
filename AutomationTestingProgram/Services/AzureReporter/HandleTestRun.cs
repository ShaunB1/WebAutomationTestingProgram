using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Graph.Client;

namespace AutomationTestingProgram.Services;

public class HandleTestRun : AzureReporter
{
    public HandleTestRun(string uri, string pat, string projectName) : base(uri, pat, projectName) {}

    public async Task<TestRun> CreateTestRunAsync(TestPlan testPlan, string runName)
    {
        if (testPlan == null)
        {
            throw new InvalidOperationException("Cannot create a test run without a test plan yet.");
        }

        var runCreateModel = new RunCreateModel(
            name: runName,
            plan: new ShallowReference { Id = testPlan.Id.ToString() },
            isAutomated: true,
            startedDate: DateTime.UtcNow.ToString("o"),
            state: "InProgress"
        );
        
        var testRun = await _managementClient.CreateTestRunAsync(runCreateModel, _projectName);
        Console.WriteLine($"Created test run {runName}");
        return testRun;
    }
}