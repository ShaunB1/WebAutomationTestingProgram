using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AutomationTestingProgram.Core;

public class AzureReporter
{
    protected readonly string _uri;
    protected readonly string _pat;
    protected readonly string _projectName;
    protected readonly VssConnection _connection;
    protected readonly WorkItemTrackingHttpClient _witClient;
    protected readonly TestPlanHttpClient _planClient;
    protected readonly TestManagementHttpClient _managementClient;

    public AzureReporter(string uri=@"https://dev.azure.com/csc-ddsb/", string pat="q4cmr4iwi6mrv6ji2w6lvnjdii4462j565bohzkccqxf73i7yd7a", string projectName="AutomationAndAccessibility")
    {
        _uri = uri;
        _pat = pat;
        _projectName = projectName;
        
        var credentials = new VssBasicCredential(string.Empty, _pat);
        _connection = new VssConnection(new Uri(_uri), credentials);
        
        _witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
        _planClient = _connection.GetClient<TestPlanHttpClient>();
        _managementClient = _connection.GetClient<TestManagementHttpClient>();        
    }
}