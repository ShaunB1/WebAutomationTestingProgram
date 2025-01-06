using System.Net.Http.Headers;
using System.Text;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;

namespace AutomationTestingProgram.Actions;

public class HandleTestRun : AzureReporter
{
    public HandleTestRun() : base() {}

    public async Task<TestRun> CreateTestRunAsync(int testPlanId, int testSuiteId, string environment, string fileName)
    {
        var testPoints = await _managementClient.GetPointsAsync(_projectName, testPlanId, testSuiteId);
        var testPointIds = new List<int>();

        foreach (var testPoint in testPoints)
        {
            testPointIds.Add(testPoint.Id);
        }
        
        var runCreateModel = new RunCreateModel(
            name: $"{fileName} [{environment}] at {DateTime.UtcNow}",
            plan: new ShallowReference { Id = testPlanId.ToString() },
            pointIds: testPointIds.ToArray(),
            state: "InProgress",
            isAutomated: true,
            startedDate: DateTime.UtcNow.ToString()
        );

        var testRun = await _managementClient.CreateTestRunAsync(runCreateModel, _projectName);
        Console.WriteLine($"Created test run {testRun.Id}");
        return testRun;
    }

    public async Task SetTestRunStateAsync(int testRunId)
    {
        var runUpdateModel = new RunUpdateModel(
            state: TestRunState.Completed.ToString()
        );
        var updatedTestRun = await _managementClient.UpdateTestRunAsync(runUpdateModel, _projectName, testRunId);
        Console.WriteLine($"Updated test run {testRunId} state to {updatedTestRun.State}");
    }
}