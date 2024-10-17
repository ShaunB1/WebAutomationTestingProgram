using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;


namespace AutomationTestingProgram.Services;

public class HandleTestPlan : AzureReporter
{
    public HandleTestPlan(string uri, string pat, string projectName) : base(uri, pat, projectName) {}

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