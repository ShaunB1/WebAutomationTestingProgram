using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using WebAutomationTestingProgram.Core.Settings.Azure;

namespace WebAutomationTestingProgram.Modules.TestRunnerOLD.Services.AzureReporter;

public class AzureReporter
{
    protected readonly string _uri;
    protected readonly string _pat;
    protected readonly string _projectName;
    protected readonly VssConnection _connection;
    protected readonly WorkItemTrackingHttpClient _witClient;
    protected readonly TestPlanHttpClient _planClient;
    protected readonly TestManagementHttpClient _managementClient;

    public AzureReporter(IOptions<AzureDevOpsSettings> options)
    {
        AzureDevOpsSettings settings = options.Value;
        _uri = settings.URI;
        _pat = settings.PAT;
        _projectName = settings.ProjectName;
        
        var credentials = new VssBasicCredential(string.Empty, _pat);
        _connection = new VssConnection(new Uri(_uri), credentials);
        
        _witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
        _planClient = _connection.GetClient<TestPlanHttpClient>();
        _managementClient = _connection.GetClient<TestManagementHttpClient>();
    }
}