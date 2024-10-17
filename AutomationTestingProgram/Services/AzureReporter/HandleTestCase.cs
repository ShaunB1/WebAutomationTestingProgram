using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AutomationTestingProgram.Services;

public class HandleTestCase : AzureReporter
{
    public HandleTestCase(string uri, string pat, string projectName) : base(uri, pat, projectName) {}
    
    public async Task<int> CreateTestCaseAsync(string testCaseName, string testCaseDescription="{DESCROPTION}")
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
        return workItem.Id ?? -1;
    }

    public async Task AddTestStepsToTestCaseAsync(int testCaseId, List<(string action, string expectedResult)> testSteps)
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
        Console.WriteLine($"Test case {testCaseId} has been updated");
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

            var tasks = new List<Task>();

            foreach (var workItemRef in queryResult.WorkItems)
            {
                tasks.Add(_managementClient.DeleteTestCaseAsync(_projectName, workItemRef.Id));
                Console.WriteLine($"Deleting test case with ID {workItemRef.Id}");
            }
            
            await Task.WhenAll(tasks);
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