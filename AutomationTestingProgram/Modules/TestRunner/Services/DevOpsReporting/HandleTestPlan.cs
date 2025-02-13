
using Humanizer;
using System.Text;
using System.Text.Json;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleTestPlan
{
    AzureReporter _azureReporter;

    public HandleTestPlan(AzureReporter azureReporter)
    {
        _azureReporter = azureReporter;
    }

    public async Task<TestPlan> GetOrCreateTestPlanAsync(string testPlanName)
    {
        try
        {
            return await GetTestPlanByNameAsync(testPlanName);
        }
        catch (NoMatchFoundException)
        {
            var newTestPlan = new
            {
                name = testPlanName
            };

            var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans?api-version=7.0";

            var jsonContent = new StringContent(JsonSerializer.Serialize(newTestPlan), Encoding.UTF8, "application/json");
            var response = await _azureReporter.jsonClient.PostAsync(requestUri, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TestPlan>(responseData)!;
            }
            else
            {
                throw new Exception($"Failed to create test plan: {response.ReasonPhrase}");
            }
            
        }
    }

    public async Task<TestPlan> GetTestPlanByNameAsync(string testPlanName)
    {
        string? continuationToken = null;
        var baseRequestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans?api-version=7.0";

        do
        {
            string requestUri = baseRequestUri;
            if (!string.IsNullOrEmpty(continuationToken))
            {
                requestUri += $"&continuationToken={continuationToken}";
            }

            var response = await _azureReporter.jsonClient.GetAsync(requestUri);
            
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var testPlansResponse = JsonSerializer.Deserialize<TestPlanListResponse>(responseData);

                var matchingPlan = testPlansResponse?.value?
                .FirstOrDefault(p => p.name.Equals(testPlanName, StringComparison.OrdinalIgnoreCase));

                if (matchingPlan != null)
                {
                    return matchingPlan;
                }

                try
                {
                    continuationToken = response.Headers.GetValues("x-ms-continuationtoken").FirstOrDefault();
                }
                catch
                {
                    continuationToken = null;
                }
            }
            else
            {
                throw new Exception($"Failed to receive test plans: {response.ReasonPhrase}");
            }

        } while (!string.IsNullOrEmpty(continuationToken));

        throw new NoMatchFoundException($"No matching test plan found for test plan '{testPlanName}'");
    }

    public async Task<TestPlan> InitializeTestPlanAsync(string testPlanName)
    {
        var testPlan = await GetOrCreateTestPlanAsync(testPlanName);
        return testPlan;
    }

    public async Task DeleteTestPlan(int testPlanId)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/testplan/plans/{testPlanId}?api-version=7.0";

        var response = await _azureReporter.jsonClient.DeleteAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            return;
        }
        else
        {
            throw new Exception($"Failed to delete test plan: {response.ReasonPhrase}");
        }
    }  
}

public class TestPlanListResponse
{
    public List<TestPlan> value { get; set; }
}

public class TestPlan
{
    public int id { get; set; }
    public Project project { get; set; }
    public DateTime updatedDate { get; set; }
    public UpdatedBy updatedBy { get; set; }
    public RootSuite rootSuite { get; set; }
    public Links _links { get; set; }
    public int revision { get; set; }
    public string name { get; set; }
    public string areaPath { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public string iteration { get; set; }
    public Owner owner { get; set; }
    public string state { get; set; }
    public TestOutcomeSettings testOutcomeSettings { get; set; }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string visibility { get; set; }
        public DateTime lastUpdateTime { get; set; }
    }

    public class UpdatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public UpdatedByLinks _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class UpdatedByLinks
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class RootSuite
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Links
    {
        public Link _self { get; set; }
        public Link clientUrl { get; set; }
        public Link rootSuite { get; set; }
        public Link build { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }

    public class Owner
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public OwnerLinks _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class OwnerLinks
    {
        public Avatar avatar { get; set; }
    }

    public class TestOutcomeSettings
    {
        public bool syncOutcomeAcrossSuites { get; set; }
    }
}




