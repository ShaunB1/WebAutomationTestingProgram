using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;
using System.Management.Automation.Language;
using System.Net.Http.Headers;
using System.Text;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

/// <summary>
/// This class sets up basic properties for reporting to devops.
/// </summary>
public class AzureReporter
{
    public readonly string uri;
    public readonly string pat;
    public readonly string projectName;
    public readonly HttpClient jsonClient;
    public readonly HttpClient jsonPatchClient;

    private readonly HashSet<string> planNames = new HashSet<string>();

    public AzureReporter(IOptions<AzureDevOpsSettings> options, IHttpClientFactory httpClientFactory)
    {
        AzureDevOpsSettings settings = options.Value;
        uri = settings.URI;
        pat = settings.PAT;
        projectName = settings.ProjectName;

        jsonClient = httpClientFactory.CreateClient("JsonClient");
        jsonClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));

        jsonPatchClient = httpClientFactory.CreateClient("JsonPatchClient");
        jsonPatchClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));
    }    
}