using System.Text;
using System.Text.Json;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleTestCase
{
    AzureReporter _azureReporter;
    
    public HandleTestCase(AzureReporter azureReporter)
    {
        _azureReporter = azureReporter;
    }

    public async Task<int> CreateTestCaseAsync(int testPlanId, int testSuiteId, string testCaseName, string testCaseDescription = "{DESCRIPTION}")
    {
        var newWorkItem = new[]
        {
            new
            {
                op = "add",
                path = "/fields/System.Title",
                from = (string)null,
                value = testCaseName
            },

            new
            {
                op = "add",
                path = "/fields/System.Description",
                from = (string)null,
                value = testCaseDescription
            },

            new
            {
                op = "add",
                path = "/fields/Microsoft.VSTS.TCM.AutomationStatus",
                from = (string)null,
                value = "Planned"
            }
        };

        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/wit/workitems/$Test%20Case?api-version=7.0";

        var jsonContent = new StringContent(JsonSerializer.Serialize(newWorkItem), Encoding.UTF8, "application/json-patch+json");
        var response = await _azureReporter.jsonPatchClient.PostAsync(requestUri, jsonContent);

        WorkItem workItem;
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            workItem = JsonSerializer.Deserialize<WorkItem>(responseData)!;
        }
        else
        {
            throw new Exception($"Failed to create work item: {response.ReasonPhrase}");
        }

        await AddTestCaseToTestSuite(testPlanId, testSuiteId, workItem.id);
        return workItem.id;
    }

    public async Task AddTestCaseToTestSuite(int testPlanId, int testSuiteId, int testCaseId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Plans/{testPlanId}/suites/{testSuiteId}/testcases/{testCaseId}?api-version=7.0";
        
        var response = await _azureReporter.jsonClient.PostAsync(requestUri, null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to link test case to test suite: {response.ReasonPhrase}");
        }
    }

    public async Task AddTestStepsToTestCaseAsync(int testCaseId, IList<TestStepObject> testSteps)
    {
        var stepsXml = Helpers.GenerateStepsXml(testSteps);
        var linkStepsToCase = new[]
        {
            new
            {
                op = "add",
                path = "/fields/Microsoft.VSTS.TCM.Steps",
                from = (string)null,
                value = stepsXml
            }
        };

        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/wit/workitems/{testCaseId}?api-version=7.0";
        var jsonContent = new StringContent(JsonSerializer.Serialize(linkStepsToCase), Encoding.UTF8, "application/json-patch+json");

        var response = await _azureReporter.jsonPatchClient.PatchAsync(requestUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to link test steps to test case: {response.ReasonPhrase}");
        }
    }

    public async Task DeleteTestCasesAsync(HandleTestSuite suiteHandler, int testPlanId)
    {
        try
        {
            var testSuites = await suiteHandler.GetAllSuitesFromPlan(testPlanId);
            var uniqueWorkItemIds = new HashSet<int>();

            foreach (var suite in testSuites)
            {
                var testCases = await GetAllCasesFromSuite(testPlanId, suite.id);                
                
                foreach (var testCase in testCases)
                {
                    uniqueWorkItemIds.Add(testCase.workItem.id);
                }

            }

            if (!uniqueWorkItemIds.Any())
            {
                return;
            }

            foreach (var workItemId in uniqueWorkItemIds)
            {
                await DeleteTestCaseAsync(workItemId);
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<List<TestCase>> GetAllCasesFromSuite(int testPlanId, int testSuiteId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans/{testPlanId}/suites/{testSuiteId}/TestCase?api-version=7.0";

        var response = await _azureReporter.jsonClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TestCasesResponse>(responseData)!.value;
        }
        else
        {
            throw new Exception($"Failed to receive test cases: {response.ReasonPhrase}");
        }
    }

    public async Task DeleteTestCaseAsync(int testCaseId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/testcases/{testCaseId}?api-version=7.0";

        var response = await _azureReporter.jsonClient.DeleteAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            return;
        }
        else
        {
            throw new Exception($"Failed to delete test case: {response.ReasonPhrase}");
        }
    }
}

public class TestCasesResponse
{
    public List<TestCase> value { get; set; }
    public int count { get; set; }
}

public class TestCase
{
    public TestPlan testPlan { get; set; }
    public Project project { get; set; }
    public TestSuite testSuite { get; set; }
    public WorkItem workItem { get; set; }
    public List<PointAssignment> pointAssignments { get; set; }
    public Links links { get; set; }

    public class TestPlan
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string visibility { get; set; }
        public DateTime lastUpdateTime { get; set; }
    }

    public class TestSuite
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class WorkItemField
    {
        public string MicrosoftVSTSCommonActivatedBy { get; set; }
        public DateTime MicrosoftVSTSCommonActivatedDate { get; set; }
        public string MicrosoftVSTSTCMAutomationStatus { get; set; }
        public string SystemDescription { get; set; }
        public string SystemState { get; set; }
        public string SystemAssignedTo { get; set; }
        public int MicrosoftVSTSCommonPriority { get; set; }
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }
        public string SystemWorkItemType { get; set; }
        public int SystemRev { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<WorkItemField> workItemFields { get; set; }
    }

    public class Tester
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public AvatarWrapper _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class AvatarWrapper
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class PointAssignment
    {
        public int id { get; set; }
        public string configurationName { get; set; }
        public Tester tester { get; set; }
        public int configurationId { get; set; }
    }

    public class Links
    {
        public LinkObject testPoints { get; set; }
        public LinkObject configuration { get; set; }
        public LinkObject _self { get; set; }
        public LinkObject sourcePlan { get; set; }
        public LinkObject sourceSuite { get; set; }
        public LinkObject sourceProject { get; set; }
    }

    public class LinkObject
    {
        public string href { get; set; }
    }
}

public class WorkItem
{
    public int id { get; set; }
    public int rev { get; set; }
    public Fields fields { get; set; }
    public Links _links { get; set; }
    public string url { get; set; }

    public class Fields
    {
        public string SystemAreaPath { get; set; }
        public string SystemTeamProject { get; set; }
        public string SystemIterationPath { get; set; }
        public string SystemWorkItemType { get; set; }
        public string SystemState { get; set; }
        public string SystemReason { get; set; }
        public AssignedTo SystemAssignedTo { get; set; }
        public DateTime SystemCreatedDate { get; set; }
        public CreatedBy SystemCreatedBy { get; set; }
        public DateTime SystemChangedDate { get; set; }
        public ChangedBy SystemChangedBy { get; set; }
        public int SystemCommentCount { get; set; }
        public string SystemTitle { get; set; }
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }
        public DateTime MicrosoftVSTSCommonActivatedDate { get; set; }
        public ActivatedBy MicrosoftVSTSCommonActivatedBy { get; set; }
        public int MicrosoftVSTSCommonPriority { get; set; }
        public string MicrosoftVSTSTCMAutomationStatus { get; set; }
        public string SystemDescription { get; set; }
    }

    public class AssignedTo
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public AssignedToLinks _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class AssignedToLinks
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class CreatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public CreatedByLinks _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class CreatedByLinks
    {
        public Avatar avatar { get; set; }
    }

    public class ChangedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public ChangedByLinks _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class ChangedByLinks
    {
        public Avatar avatar { get; set; }
    }

    public class ActivatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public ActivatedByLinks _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class ActivatedByLinks
    {
        public Avatar avatar { get; set; }
    }

    public class Links
    {
        public Link self { get; set; }
        public Link workItemUpdates { get; set; }
        public Link workItemRevisions { get; set; }
        public Link workItemComments { get; set; }
        public Link html { get; set; }
        public Link workItemType { get; set; }
        public Link fields { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }
}