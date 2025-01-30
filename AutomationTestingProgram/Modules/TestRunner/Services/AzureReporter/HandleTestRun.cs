//using Microsoft.TeamFoundation.TestManagement.WebApi;

//namespace AutomationTestingProgram.Actions;

//public class HandleTestRun : AzureReporter
//{
//    public async Task<TestRun> CreateTestRunAsync(int testPlanId, int testSuiteId, string environment, string fileName)
//    {
//        var testPoints = await _managementClient.GetPointsAsync(_projectName, testPlanId, testSuiteId);
//        var testPointIds = new List<int>();

//        foreach (var testPoint in testPoints)
//        {
//            testPointIds.Add(testPoint.Id);
//        }
        
//        var runCreateModel = new RunCreateModel(
//            name: $"{fileName} [{environment}] at {DateTime.UtcNow}",
//            plan: new ShallowReference { Id = testPlanId.ToString() },
//            pointIds: testPointIds.ToArray(),
//            state: "InProgress",
//            isAutomated: true,
//            startedDate: DateTime.UtcNow.ToString()
//        );

//        var testRun = await _managementClient.CreateTestRunAsync(runCreateModel, _projectName);
//        Console.WriteLine($"Created test run {testRun.Id}");
//        return testRun;
//    }

//    public async Task SetTestRunStateAsync(int testRunId)
//    {
//        var runUpdateModel = new RunUpdateModel(
//            state: TestRunState.Completed.ToString()
//        );
//        var updatedTestRun = await _managementClient.UpdateTestRunAsync(runUpdateModel, _projectName, testRunId);
//        Console.WriteLine($"Updated test run {testRunId} state to {updatedTestRun.State}");
//    }
//}