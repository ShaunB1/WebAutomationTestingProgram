using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AutomationTestingProgram.Services;

public class AzureReporter
{
    protected readonly string _uri;
    protected readonly string _pat;
    protected readonly string _projectName;
    protected readonly VssConnection _connection;
    protected readonly WorkItemTrackingHttpClient _witClient;
    protected readonly TestPlanHttpClient _planClient;
    protected readonly TestManagementHttpClient _managementClient;

    public AzureReporter(string uri, string pat, string projectName)
    {
        _uri = uri;
        _pat = pat;
        _projectName = projectName;
        
        var credentials = new VssBasicCredential(string.Empty, _pat);
        _connection = new VssConnection(new Uri(_uri), credentials);
        Console.WriteLine($"Connected to {_uri}");
        
        _witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
        _planClient = _connection.GetClient<TestPlanHttpClient>();
        _managementClient = _connection.GetClient<TestManagementHttpClient>();        
    }
}