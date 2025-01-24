using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;

namespace AutomationTestingProgram.Actions;

public class HandleTestPlan : AzureReporter
{
    public async Task<TestPlan> GetOrCreateTestPlanAsync(string testPlanName)
    {
        var allTestPlans = new List<TestPlan>();
        string continuationToken = null;

        do
        {
            var testPlanList = await _planClient.GetTestPlansAsync(
                project: _projectName,
                continuationToken: continuationToken
            );
            allTestPlans.AddRange(testPlanList);
            continuationToken = testPlanList.ContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));
        
        var existingTestPlan = allTestPlans.FirstOrDefault(p => p.Name.Equals(testPlanName, StringComparison.OrdinalIgnoreCase));

        if (existingTestPlan != null)
        {
            Console.WriteLine($"TestPlan {existingTestPlan.Name} already exists");
            return existingTestPlan;
        }
        
        var newTestPlan = new TestPlanCreateParams { Name = testPlanName };
        var createdPlan = await _planClient.CreateTestPlanAsync(newTestPlan, _projectName);
        
        Console.WriteLine($"Created new test plan {createdPlan.Name}");
        return createdPlan;
    }

    public async Task<int> GetTestPlanIdByNameAsync(string testPlanName)
    {
        var allTestPlans = new List<TestPlan>();
        string continuationToken = null;

        do
        {
            var testPlanList = await _planClient.GetTestPlansAsync(_projectName, continuationToken: continuationToken, includePlanDetails: true);

            if (testPlanList.Any())
            {
                allTestPlans.AddRange(testPlanList);
            }
            
            continuationToken = testPlanList.ContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));
        
        var matchingPlan = allTestPlans.FirstOrDefault(p => p.Name.Equals(testPlanName, StringComparison.OrdinalIgnoreCase));

        if (matchingPlan != null)
        {
            Console.WriteLine($"Found matching test plan '{testPlanName}'");
            return matchingPlan.Id;
        }
        else
        {
            Console.WriteLine($"No matching test plan found for test plan '{testPlanName}'");
            return -1;
        }
    }

    public async Task<(TestPlan, Microsoft.TeamFoundation.TestManagement.WebApi. TestSuite)> InitializeTestPlanAsync(string testPlanName)
    {
        var suiteHandler = new HandleTestSuite();
        var testPlan = await GetOrCreateTestPlanAsync(testPlanName);
        var fileSuite = await suiteHandler.TestSuiteSetupAsync(testPlan.Id, "App Name", "Release", "Test File");
        return (testPlan, fileSuite);
    }

    public async Task DeleteTestPlan(string testPlanName)
    {
        try
        {
            var allTestPlans = new List<TestPlan>();
            string continuationToken = null;

            do
            {
                var testPlanList = await _planClient.GetTestPlansAsync(_projectName, owner: "Shaun Bautista", continuationToken: continuationToken, includePlanDetails: true);
                allTestPlans.AddRange(testPlanList);
                continuationToken = testPlanList.ContinuationToken;
            } while (!string.IsNullOrEmpty(continuationToken));
            {
                var testPlanToDelete = allTestPlans.FirstOrDefault(p => p.Name.Equals(testPlanName, StringComparison.OrdinalIgnoreCase));

                if (testPlanToDelete != null)
                {
                    await _planClient.DeleteTestPlanAsync(_projectName, testPlanToDelete.Id);
                    Console.WriteLine($"Deleted test plan '{testPlanName}'");
                }
                else
                {
                    Console.WriteLine($"Test plan {testPlanName} does not exist");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}