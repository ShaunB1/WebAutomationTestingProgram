using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AutomationTestingProgram.Actions;

public class HandleTestCase : AzureReporter
{
    public HandleTestCase() : base() {}
        
    public async Task<int> CreateTestCaseAsync(string testCaseName, string testCaseDescription="{DESCRIPTION}")
    {
        var patchDocument = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = testCaseName
            },
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Description",
                Value = testCaseDescription
            },
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.AutomationStatus",
                Value = "Planned"
            }
        };
        
        var workItem = await _witClient.CreateWorkItemAsync(patchDocument, _projectName, "Test Case");

        if (workItem == null)
        {
            throw new Exception("Failed to create test case.");
        }
        
        return workItem.Id ?? -1;
    }

    // public async Task RecordTestCaseResultWithTestStepsAsync(int testRunId, 
    //     Microsoft.TeamFoundation.TestManagement.WebApi.TestPoint testPoint, string testCaseName, int testCaseId, string overallOutcome)
    // {
    //     var testCase = await _witClient.GetWorkItemAsync(testCaseId);
    //     var latestRevision = testCase.Rev;
    //
    //     var testCaseResult = new TestCaseResult
    //     {
    //         TestCaseTitle = testCaseName,
    //         TestCase = new ShallowReference { Id = testCaseId.ToString() },
    //         TestPoint = new ShallowReference { Id = testPoint.Id.ToString() },
    //         TestCaseRevision = latestRevision ?? 0,
    //         Outcome = overallOutcome,
    //         State = "Completed",
    //         StartedDate = DateTime.UtcNow,
    //         CompletedDate = DateTime.UtcNow.AddMinutes(1),
    //         Comment = "Adding detailed information to test result.",
    //         ErrorMessage = "Error message",
    //         StackTrace = "Stack trace details here...",
    //         FailureType = "Regression",
    //     };
    //     
    //     var testResults = new List<TestCaseResult> { testCaseResult };
    //     await _managementClient.AddTestResultsToTestRunAsync(
    //         testResults.ToArray(),
    //         _projectName,
    //         testRunId
    //     );
    // }

    public async Task AddTestStepsToTestCaseAsync(int testCaseId, List<TestStep> testSteps)
    {
        var stepsXml = Helpers.GenerateStepsXml(testSteps);
        var patchDocument = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.Steps",
                Value = stepsXml
            }
        };
        var updatedWorkItem = await _witClient.UpdateWorkItemAsync(patchDocument, testCaseId);
        Console.WriteLine($"Updated Test Case {testCaseId} with test steps.");
    }
    
    public async Task DeleteAllTestCasesInTestPlanAsync(int testPlanId)
    {
        var testSuites = await _planClient.GetTestSuitesForPlanAsync(_projectName, testPlanId);
        var uniqueTestCases = new HashSet<int>();

        foreach (var suite in testSuites)
        {
            var testCaseList = await _planClient.GetTestCaseListAsync(_projectName, testPlanId, suite.Id);
            
            if (testCaseList.Any())
            {
                foreach (var testCase in testCaseList)
                {
                    uniqueTestCases.Add(testCase.workItem.Id);
                }
            }
        }
        
        if (!uniqueTestCases.Any())
        {
            Console.WriteLine("No test cases were found");
            return;
        }

        var tasks = new List<Task>();
        
        foreach (var testCaseId in uniqueTestCases)
        {
            tasks.Add(_managementClient.DeleteTestCaseAsync(_projectName, testCaseId));
        }

        await Task.WhenAll(tasks);
        tasks.Clear();
        Console.WriteLine("Test cases successfully deleted.");
    }

    public async Task CreateAndAddTestCaseToTestSuiteAsync(int testPlanId, int testSuiteId, string testCaseName)
    {
        var testSuiteHandler = new HandleTestSuite();
        var testCaseId = await CreateTestCaseAsync(testCaseName);
        await testSuiteHandler.AddTestCaseToTestSuite(testPlanId, testSuiteId, testCaseId);  
    }
    
    public async Task DeleteTestCasesAsync(string userName)
    {
        try
        {
            var wiql = new Wiql
            {
                Query = $@"
               SELECT [System.Id]
               FROM WorkItems
               WHERE [System.WorkItemType] = 'Test Case'
               AND [System.AssignedTo] = '{userName}'
               AND [System.TeamProject] = '{_projectName}'
            "
            };
            var queryResult = await _witClient.QueryByWiqlAsync(wiql, _projectName);

            if (!queryResult.WorkItems.Any())
            {
                Console.WriteLine($"No test runs found for {userName}");
                return;
            }

            var uniqueWorkItemIds = new HashSet<int>();
            foreach (var workItemRef in queryResult.WorkItems)
            {
                uniqueWorkItemIds.Add(workItemRef.Id);
            }

            var tasks = new List<Task>();

            foreach (var workItemId in uniqueWorkItemIds)
            {
                await _managementClient.DeleteTestCaseAsync(_projectName, workItemId);
                Console.WriteLine($"Deleting test case with ID {workItemId}");
            }
            
            // await Task.WhenAll(tasks);
            tasks.Clear();
            
            Console.WriteLine($"All assigned test cases have been deleted.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}