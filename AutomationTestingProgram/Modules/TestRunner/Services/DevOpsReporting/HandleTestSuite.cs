using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using System.Text;
using System.Text.Json;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleTestSuite
{
    AzureReporter _azureReporter;

    public HandleTestSuite(AzureReporter azureReporter)
    {
        _azureReporter = azureReporter;
    }

    public async Task<List<TestSuite>> GetAllSuitesFromPlan(int testPlanId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans/{testPlanId}/suites?api-version=7.0";

        var response = await _azureReporter.jsonClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TestSuitesResponse>(responseData)!.value;
        }
        else
        {
            throw new Exception($"Failed to receive test suites: {response.ReasonPhrase}");
        }
    }


    public async Task<TestSuite> TestSuiteSetupAsync(int planId, string appName, string releaseNumber, string fileName)
    {
        try
        {
            var rootSuite = await GetOrCreateRootSuiteAsync(planId);
            var dateSuite = await GetOrCreateTestSuiteAsync(planId, rootSuite.id, $"yyyy-mm-dd TEST 12.3 {releaseNumber} Code Freeze Date yyyy-mm-dd");
            var appSuite = await GetOrCreateTestSuiteAsync(planId, dateSuite.id, $"{appName} 2023-24");
            var buildSuite = await GetOrCreateTestSuiteAsync(planId, appSuite.id, fileName);
            
            return buildSuite;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public async Task<TestSuite> GetOrCreateTestSuiteAsync(int planId, int parentSuiteId, string testSuiteName)
    {
        var newTestSuite = new
        {
            name = testSuiteName,
            suiteType = "StaticTestSuite",
            parentSuite = new
            {
                id = parentSuiteId
            }
        };

        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans/{planId}/suites?api-version=7.0";

        var jsonContent = new StringContent(JsonSerializer.Serialize(newTestSuite), Encoding.UTF8, "application/json");
        var response = await _azureReporter.jsonClient.PostAsync(requestUri, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TestSuite>(responseData)!;
        }
        else
        {
            throw new Exception($"Failed to create test suite: {response.ReasonPhrase}");
        }
    }
    
    public async Task<TestSuite> GetOrCreateRootSuiteAsync(int planId)
    {
        var suites = await GetAllSuitesFromPlan(planId);

        if (suites.Any())
        {
            return suites.First();
        }

        var newTestSuite = new
        {
            Name = "Root",
            SuiteType = "StaticTestSuite"
        };

        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans/{planId}/suites?api-version=7.0";

        var jsonContent = new StringContent(JsonSerializer.Serialize(newTestSuite), Encoding.UTF8, "application/json");
        var response = await _azureReporter.jsonClient.PostAsync(requestUri, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TestSuite>(responseData)!;
        }
        else
        {
            throw new Exception($"Failed to create test suite: {response.ReasonPhrase}");
        }
    }   
}

public class TestSuitesResponse
{
    public List<TestSuite> value { get; set; }
    public int count { get; set; }
}

public class TestSuite
{
    public int id { get; set; }
    public int revision { get; set; }
    public Project project { get; set; }
    public DateTime lastUpdatedDate { get; set; }
    public Plan plan { get; set; }
    public Links _links { get; set; }
    public string suiteType { get; set; }
    public string name { get; set; }
    public bool inheritDefaultConfigurations { get; set; }
    public List<Configuration> defaultConfigurations { get; set; }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public string visibility { get; set; }
        public DateTime lastUpdateTime { get; set; }
    }

    public class Plan
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Links
    {
        public Link _self { get; set; }
        public Link testCases { get; set; }
        public Link testPoints { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }

    public class Configuration
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}


