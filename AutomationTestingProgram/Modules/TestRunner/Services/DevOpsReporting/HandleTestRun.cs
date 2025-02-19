using Microsoft.Azure.Pipelines.WebApi;
using System.Text;
using System.Text.Json;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleTestRun
{
    AzureReporter _azureReporter;

    public HandleTestRun(AzureReporter azureReporter)
    {
        _azureReporter = azureReporter;
    }

    public async Task<int> CreateTestRunAsync(TestRunObject testRun, ProcessRequest request)
    {
        var requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Plans/{testRun.PlanID}/Suites/{testRun.SuiteID}/points?api-version=7.0";

        var response = await _azureReporter.jsonClient.GetAsync(requestUri);

        List<TestPoint> points;
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            points = JsonSerializer.Deserialize<TestPointResponse>(responseData)!.value;
        }
        else
        {
            throw new Exception($"Failed to get test points: {response.ReasonPhrase}");
        }

        var runCreateModel = new
        {
            name = $"{request.FileName} [{request.Environment}] at {DateTime.Now}",
            plan = new { id = testRun.PlanID.ToString() },
            pointIds = points.Select(tp => tp.id).ToArray(),
            state = "InProgress",
            isAutomated = true,
            startedDate = testRun.StartedDate,
            comment = testRun.GenerateRunInformation(request),
        };

        requestUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/runs?api-version=7.0";

        var jsonContent = new StringContent(JsonSerializer.Serialize(runCreateModel), Encoding.UTF8, "application/json");
        response = await _azureReporter.jsonClient.PostAsync(requestUri, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TestRun>(responseData)!.id;
        }
        else
        {
            throw new Exception($"Failed to create test run: {response.ReasonPhrase}");
        }
    }

    public async Task SetTestRunStateAsync(TestRunObject testRun)
    {
        var resultUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Runs/{testRun.ID}?api-version=7.0";

        var runResult = new
        {
            startedDate = testRun.StartedDate,
            completedDate = testRun.CompletedDate,
            state = "Completed",
            errorMessage = $"Test Run {testRun.Result}" + (testRun.FailureCounter > 0 ? $"\nTest Case Failures: {testRun.FailureCounter}": null),
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(runResult), Encoding.UTF8, "application/json");
        var response = await _azureReporter.jsonClient.PatchAsync(resultUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update test run state: {response.ReasonPhrase}");
        }
    }

    public async Task AddAttachment(TestRunObject testRun, string comment, string folderPath, string fileName)
    {
        var resultUri = $"{_azureReporter.uri}/{_azureReporter.projectName}/_apis/test/Runs/{testRun.ID}/attachments?api-version=7.0";

        string filePath = Path.Combine(folderPath, fileName);

        if (File.Exists(filePath))
        {
            byte[]? bytes = await File.ReadAllBytesAsync(filePath);
            string? file64 = Convert.ToBase64String(bytes);

            var attachment = new
            {
                attachmentType = "GeneralAttachment",
                fileName = $"{comment}{Path.GetExtension(filePath)}",
                comment = comment,
                stream = file64
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(attachment), Encoding.UTF8, "application/json");
            var response = await _azureReporter.jsonClient.PostAsync(resultUri, jsonContent);

            bytes = null;
            file64 = null;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to update test run attachment: {response.ReasonPhrase}");
            }
        }
        else
        {
            throw new Exception("Attachment not found!");
        }        
    }
}

public class TestRun
{
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public Build build { get; set; }
    public BuildConfiguration buildConfiguration { get; set; }
    public bool isAutomated { get; set; }
    public Owner owner { get; set; }
    public Project project { get; set; }
    public DateTime startedDate { get; set; }
    public DateTime completedDate { get; set; }
    public string state { get; set; }
    public string postProcessState { get; set; }
    public int totalTests { get; set; }
    public int incompleteTests { get; set; }
    public int notApplicableTests { get; set; }
    public int passedTests { get; set; }
    public int unanalyzedTests { get; set; }
    public DateTime createdDate { get; set; }
    public DateTime lastUpdatedDate { get; set; }
    public LastUpdatedBy lastUpdatedBy { get; set; }
    public int revision { get; set; }
    public List<RunStatistic> runStatistics { get; set; }
    public string webAccessUrl { get; set; }
    public List<CustomField> customFields { get; set; }
    public PipelineReference pipelineReference { get; set; }

    public class Build
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class BuildConfiguration
    {
        public int id { get; set; }
        public string number { get; set; }
        public string uri { get; set; }
        public string flavor { get; set; }
        public string platform { get; set; }
        public int buildDefinitionId { get; set; }
        public string branchName { get; set; }
        public Project project { get; set; }
        public string targetBranchName { get; set; }
    }

    public class Owner
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class Links
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class LastUpdatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class RunStatistic
    {
        public string state { get; set; }
        public string outcome { get; set; }
        public int count { get; set; }
    }

    public class CustomField
    {
        public string fieldName { get; set; }
        public string value { get; set; }
    }

    public class PipelineReference
    {
        public int pipelineId { get; set; }
        public StageReference stageReference { get; set; }
        public PhaseReference phaseReference { get; set; }
        public JobReference jobReference { get; set; }
        public int pipelineDefinitionId { get; set; }
    }

    public class StageReference
    {
        public string stageName { get; set; }
        public int attempt { get; set; }
    }

    public class PhaseReference
    {
        public string phaseName { get; set; }
        public int attempt { get; set; }
    }

    public class JobReference
    {
        public string jobName { get; set; }
        public int attempt { get; set; }
    }
}



