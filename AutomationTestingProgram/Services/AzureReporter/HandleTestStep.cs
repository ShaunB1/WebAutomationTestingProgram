using AutomationTestingProgram.Models;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace AutomationTestingProgram.Services;

public class HandleTestStep : AzureReporter
{
    public HandleTestStep(string uri, string pat, string projectName) : base(uri, pat, projectName) {}

    public async Task AddTestStepResults(int testRunId, int testCaseId, List<TestStepResult> stepResults)
    {
        var testResults = await _managementClient.GetTestResultsAsync(
            project: _projectName,
            runId: testRunId,
            detailsToInclude: ResultDetails.WorkItems
        );

        if (testResults == null || !testResults.Any())
        {
            Console.WriteLine("No test results found");
            return;
        }
        
        var filteredTestResults = testResults.Where(tr => tr.TestCase != null && Convert.ToInt32(tr.TestCase.Id) == testCaseId).ToList();
    }
}