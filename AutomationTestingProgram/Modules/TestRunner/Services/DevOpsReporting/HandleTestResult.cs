using AutomationTestingProgram.Actions;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System.Text;
using System.Text.Json;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleTestResult
{
    AzureReporter _azureReporter;

    public HandleTestResult(AzureReporter azureReporter)
    {
        _azureReporter = azureReporter;
    }

    public async Task GetTestResultFromTestCaseIdAsync(IList<TestCaseObject> testCases, int testRunId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Runs/{testRunId}/results?api-version=7.0";
        var response = await _azureReporter.jsonClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            List<TestResult> result = JsonSerializer.Deserialize<TestResultResponse>(responseData)!.value;

            var resultLookup = result.ToDictionary(
                tr => $"{tr.testCase.id}-{tr.testPoint.id}",
                tr => tr.id
            );

            foreach (var testCase in testCases)
            {
                var key = $"{testCase.ID}-{testCase.PointID}";

                if (resultLookup.TryGetValue(key, out int resultID))
                {
                    testCase.ResultID = resultID;
                }
                else
                {
                    throw new Exception("Failed to link testResult to testCase!");
                }
            }
        }
        else
        {
            throw new Exception($"Failed to get test results: {response.ReasonPhrase}");
        }
    }

    public async Task UpdateTestCaseResultAsync(int testRunId, int testResultId, TestCaseObject testCase)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Runs/{testRunId}/results?api-version=7.0";
        var caseResult = new
        {
            id = testResultId,
            startedDate = testCase.StartedDate,
            completedDate = testCase.CompletedDate,
            errorMessage = testCase.Result == Result.Failed ? "Something went wrong" : null,
            outcome = testCase.Result.ToString(),
            state = TestRunState.Completed,
            durationInMs = Convert.ToInt32((testCase.CompletedDate - testCase.StartedDate).TotalMilliseconds)
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(new[] { caseResult }), Encoding.UTF8, "application/json");
        var response = await _azureReporter.jsonClient.PatchAsync(requestUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update test case result: {response.ReasonPhrase}");
        }
    }

    public async Task UpdateTestStepResultAsync(int testRunId, int testResultId, TestStepObject testStep, Exception? e = null)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Runs/{testRunId}/results?api-version=7.0";
        var stepResult = new
        {
            id = testResultId,
            state = TestRunState.InProgress,
            outcome = "InProgress",
            iterationDetails = new
            {
                actionResults = new
                {
                    stepIdentifier = testStep.StepNum,
                    completedDate = testStep.CompletedDate,
                    startedDate = testStep.StartedDate,
                    durationInMs = Convert.ToInt32((testStep.CompletedDate - testStep.StartedDate).TotalMilliseconds),
                    outcome = testStep.Result.ToString(),
                    comment = $"Description: {testStep.TestDescription}, Total Attempts Used: {testStep.FailureCounter + 1}",
                    errorMessage = e != null ? $"Error Message: {e.Message}\n Stack Trace: {e.StackTrace}" : null,
                }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(new[] { stepResult }), Encoding.UTF8, "application/json");
        var response = await _azureReporter.jsonClient.PatchAsync(requestUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update test step result: {response.ReasonPhrase}");
        }
    }
}

public class TestResultResponse
{
    public List<TestResult> value { get; set; }
    public int count { get; set; }
}

public class TestResult
{
    public int id { get; set; }
    public Configuration configuration { get; set; }
    public Project project { get; set; }
    public DateTime startedDate { get; set; }
    public DateTime completedDate { get; set; }
    public int revision { get; set; }
    public string state { get; set; }
    public TestCase testCase { get; set; }
    public TestPoint testPoint { get; set; }
    public TestRun testRun { get; set; }
    public DateTime lastUpdatedDate { get; set; }
    public int priority { get; set; }
    public DateTime createdDate { get; set; }
    public string url { get; set; }
    public string failureType { get; set; }
    public string testCaseTitle { get; set; }
    public int testCaseRevision { get; set; }
    public List<CustomField> customFields { get; set; }
    public TestPlan testPlan { get; set; }
    public int testCaseReferenceId { get; set; }
    public Owner owner { get; set; }
    public RunBy runBy { get; set; }
    public LastUpdatedBy lastUpdatedBy { get; set; }

    public class Configuration
    {
        public string id { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class TestCase
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class TestPoint
    {
        public string id { get; set; }
    }

    public class TestRun
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class CustomField
    {
        
    }

    public class TestPlan
    {
        public string id { get; set; }
    }

    public class Owner
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class RunBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class LastUpdatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class Links
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }
}




