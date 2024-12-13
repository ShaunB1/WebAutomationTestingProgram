using AutomationTestingProgram.Models;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi.Legacy;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AutomationTestingProgram.Services;

public class HandleTestStep : AzureReporter
{
    public HandleTestStep() : base() {}
    
    public async Task AddTestStepsToTestCaseAsync(int testCaseId, List<TestStepV1> testSteps)
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

    // public async Task RecordTestStepOutcomesAsync(int testPlanId, int testRunId, int testSuiteId, int testCaseId)
    // {
    //     Console.WriteLine("TEST PLAN: ", testPlanId);
    //     var testPoints = await _managementClient.GetPointsAsync(_projectName, testPlanId, testSuiteId, testCaseId: testCaseId.ToString());
    //     var testPoint = testPoints.FirstOrDefault();
    //     var testCaseDetails = await _managementClient.GetTestCaseByIdAsync(_projectName, testPlanId, testSuiteId, testCaseId);
    //     var testCase = testCaseDetails.Workitem;
    //     var workItem = await _witClient.GetWorkItemAsync(testCaseId, expand: WorkItemExpand.Relations);
    //     var revisionNumber = workItem.Rev ?? -1;
    //
    //     var testStep = new List<TestActionResultModel>
    //     {
    //         new TestActionResultModel
    //         {
    //             ActionPath = "1",
    //             Outcome = "Passed",
    //             DurationInMs = 1000,
    //             ErrorMessage = "Error",
    //         },
    //         new TestActionResultModel
    //         {
    //             ActionPath = "2",
    //             Outcome = "Failed",
    //             DurationInMs = 1500,
    //             ErrorMessage = "Some validation failed."
    //         }
    //     };
    //     
    //     var testStepResults = new List<TestActionResult2>
    //     {
    //         new TestActionResult2
    //         {
    //             ActionPath = "1", // Test step 1
    //             Outcome = 1,
    //             Duration = 1000,
    //             ErrorMessage = null
    //         },
    //         new TestActionResult2
    //         {
    //             ActionPath = "2", // Test step 2
    //             Outcome = 0,
    //             Duration = 2000,
    //             ErrorMessage = "Validation failed."
    //         }
    //     };
    //     var testCaseResult = new TestCaseResult
    //     {
    //         TestPoint = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference
    //         {
    //             Id = testPoint.Id.ToString()
    //         },
    //         TestCase = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference
    //         {
    //             Id = testCaseId.ToString(),
    //         },
    //         TestRun = new ShallowReference { Id = testRunId.ToString() },
    //         Outcome = "Passed",
    //         TestCaseRevision = revisionNumber,
    //         TestCaseTitle = testCase.Name ?? "Test Title",
    //         IterationDetails = new List<TestIterationDetailsModel>
    //         {
    //             new TestIterationDetailsModel
    //             {
    //                 Id = 1, // Iteration 1
    //                 Outcome = "Failed", // Overall outcome for the iteration
    //                 DurationInMs = 3000,
    //                 ActionResults = testStep // Include the test steps and their outcomes
    //             }
    //         },
    //         SubResults = new List<TestSubResult>
    //         {
    //             new TestSubResult
    //             {
    //                 SequenceId = 1,
    //                 Outcome = "Passed",
    //                 DurationInMs = 1000,
    //                 ErrorMessage = null
    //             },
    //             new TestSubResult
    //             {
    //                 SequenceId = 2,
    //                 Outcome = "Passed",
    //                 DurationInMs = 2000,
    //                 ErrorMessage = "Validation failed."
    //             }
    //         },
    //
    //         StartedDate = DateTime.UtcNow,
    //         CompletedDate = DateTime.UtcNow.AddMinutes(1),
    //     };
    //     
    //     TestCaseResult[] resultsArray = new[] { testCaseResult };
    //     
    //     var createdResults = await _managementClient.AddTestResultsToTestRunAsync(
    //         results: resultsArray, 
    //         project: _projectName, 
    //         runId: testRunId
    //     );
    // }
}