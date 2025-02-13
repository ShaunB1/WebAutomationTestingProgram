using System.Text.Json;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleTestPoint
{
    AzureReporter _azureReporter;

    public HandleTestPoint(AzureReporter azureReporter)
    {
        _azureReporter = azureReporter;
    }

    public async Task<int> GetTestPointFromTestCaseIdAsync(int testPlanID, int testSuiteID, int testCaseId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/plans/{testPlanID}/suites/{testSuiteID}/points?testCaseId={testCaseId}&api-version=7.0";
        var response = await _azureReporter.jsonClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TestPointResponse>(responseData)!.value.First().id;
        }
        else
        {
            throw new Exception($"Failed to get test point: {response.ReasonPhrase}");
        }
    }
}

public class TestPointResponse
{
    public List<TestPoint> value { get; set; }
    public int count { get; set; }
}

public class TestPoint
{
    public int id { get; set; }
    public string url { get; set; }
    public AssignedTo assignedTo { get; set; }
    public bool automated { get; set; }
    public Configuration configuration { get; set; }
    public LastTestRun lastTestRun { get; set; }
    public LastResult lastResult { get; set; }
    public string outcome { get; set; }
    public string state { get; set; }
    public string lastResultState { get; set; }
    public TestCase testCase { get; set; }
    public List<WorkItemProperty> workItemProperties { get; set; }

    public class AssignedTo
    {
        public string displayName { get; set; }
        public string id { get; set; }
    }

    public class Configuration
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class LastTestRun
    {
        public string id { get; set; }
    }

    public class LastResult
    {
        public string id { get; set; }
    }

    public class TestCase
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string webUrl { get; set; }
    }

    public class WorkItemProperty
    {
        public WorkItem workItem { get; set; }
    }

    public class WorkItem
    {
        public string key { get; set; }
        public object value { get; set; }
    }
}