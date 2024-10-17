using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;


namespace AutomationTestingProgram.Services;

public class HandleTestSuite : AzureReporter
{
    public HandleTestSuite(string uri, string pat, string projectName) : base(uri, pat, projectName) {}
    
    public async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite> TestSuiteSetupAsync(int planId, string appName, string releaseNumber, string fileName)
    {
        try
        {
            var rootSuite = await GetOrCreateRootSuiteAsync(planId);
            var dateSuite = await GetOrCreateTestSuiteAsync(planId, $"yyyy-mm-dd TEST 12.3 {releaseNumber} Code Freeze Date yyyy-mm-dd", rootSuite.Id);
            var appSuite = await GetOrCreateTestSuiteAsync(planId, $"{appName} 2023-24", dateSuite.Id);
            var buildSuite = await GetOrCreateTestSuiteAsync(planId, "buildNumber", appSuite.Id);
            
            Console.WriteLine("Successfully set up test suites.");
            return buildSuite;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite> GetOrCreateTestSuiteAsync(int planId, string testSuiteName, int parentSuiteId)
    {
        var suiteCreateModel = new SuiteCreateModel(
            suiteType: TestSuiteType.StaticTestSuite.ToString(),
            name: testSuiteName
        );
        var createdSuite = await _managementClient.CreateTestSuiteAsync(
            suiteCreateModel,
            _projectName,
            planId,
            parentSuiteId
        );
        return createdSuite.FirstOrDefault();
    }
    
    public async Task<TestSuite> GetOrCreateRootSuiteAsync(int planId)
    {
        var suites = await _planClient.GetTestSuitesForPlanAsync(_projectName, planId);

        if (suites.Any())
        {
            var rootSuite = suites.First();
            return rootSuite;
        }

        var suiteCreateParams = new TestSuiteCreateParams
        {
            Name = "Root Suite",
            SuiteType = TestSuiteType.StaticTestSuite
        };
        var newRootSuite = await _planClient.CreateTestSuiteAsync(
            suiteCreateParams,
            _projectName,
            planId
        );

        Console.WriteLine($"Created new test suite: {newRootSuite.Name}");
        return newRootSuite;
    }

    public async Task AddTestCaseToTestSuiteAsync(int suiteId, int testCaseId, TestPlan testPlan)
    {
        var configurations = await _planClient.GetTestConfigurationsAsync(_projectName);
        var configuration = configurations.First();

        if (configuration == null)
        {
            throw new Exception($"No configuration found for {_projectName}");
        }

        var configurationList = new List<Configuration>
        {
            new Configuration { ConfigurationId = configuration.Id }
        };

        var parameters = new List<SuiteTestCaseCreateUpdateParameters>
        {
            new SuiteTestCaseCreateUpdateParameters
            {
                PointAssignments = configurationList,
                workItem = new Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem
                {
                    Id = testCaseId
                },
            }
        };

        await _planClient.AddTestCasesToSuiteAsync(
            parameters,
            _projectName,
            testPlan.Id,
            suiteId
        );
        
        Console.WriteLine($"Added test case for {suiteId}");
    }
}