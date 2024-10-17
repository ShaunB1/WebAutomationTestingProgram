using System.Net.Http.Headers;
using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation;

namespace AutomationTestingProgram.Services;

public class AzureDevOpsReporter
{
   private readonly string _uri;
   private readonly string _pat;
   private readonly string _projectName;
   private readonly VssConnection _connection;
   private TestPlan _testPlan;
   private TestRun _testRun;

   public AzureDevOpsReporter(string uri, string pat, string projectName)
   {
      _uri = uri;
      _pat = pat;
      _projectName = projectName;

      var credentials = new VssBasicCredential(string.Empty, _pat);
      _connection = new VssConnection(new Uri(uri), credentials);
      Console.WriteLine($"Connected to {_uri}");
   }
   
   public async Task<int> CreateTestCaseAsync(string testCaseName, string testCaseDescription="")
   {
      var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
      var patchDocument = new JsonPatchDocument
      {
         new JsonPatchOperation
         {
            Operation = Operation.Add,
            Path = "/fields/System.Title",
            Value = testCaseName,
         },
         new JsonPatchOperation
         {
            Operation = Operation.Add,
            Path = "/fields/System.Description",
            Value = testCaseDescription,
         },
         new JsonPatchOperation
         {
            Operation = Operation.Add,
            Path = "/fields/Microsoft.VSTS.TCM.AutomationStatus",
            Value = "Planned",
         }
      };
      var workItem = await witClient.CreateWorkItemAsync(patchDocument, _projectName, "Test Case");
      Console.WriteLine($"Created test case for {testCaseName}");
      return workItem.Id ?? -1;
   }

   public async Task RecordTestCaseResultAsync(int suiteId, string testCaseName, int testCaseId, string outcome)
   {
      var client = _connection.GetClient<TestManagementHttpClient>();
      var testPoint = await GetTestPointForTestCaseAsync(suiteId, testCaseId);
      var testCaseResult = new TestCaseResult
      {
         TestCase = new ShallowReference { Id = testCaseId.ToString() },
         TestPoint = new ShallowReference{ Id = testPoint.Id.ToString() },
         TestCaseRevision = testPoint.Revision,
         TestCaseTitle = testCaseName,
         Outcome = outcome,
         State = "Completed",
         StartedDate = DateTime.UtcNow,
      };
      var res = new List<TestCaseResult> { testCaseResult };
      await client.AddTestResultsToTestRunAsync(res.ToArray(), _projectName, _testRun.Id);
      Console.WriteLine($"Recorded Test Case Result: {outcome}");
   }

   public async Task AddTestCaseToSuiteAsync(int suiteId, int testCaseId)
   {
      var client = _connection.GetClient<TestPlanHttpClient>();
      var configurations = await client.GetTestConfigurationsAsync(_projectName);
      var configuration = configurations.FirstOrDefault();

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

      await client.AddTestCasesToSuiteAsync(
         parameters,
         _projectName,
         _testPlan.Id,
         suiteId,
         userState: null,
         cancellationToken: CancellationToken.None
      );
      Console.WriteLine($"Added test case for {testCaseId}");
   }

   public async Task AddTestStepsToTestCaseAsync(int testCaseId, List<(string action, string expectedResult)> testSteps)
   {
      var client = _connection.GetClient<WorkItemTrackingHttpClient>();
      var stepsXml = GenerateStepsXml(testSteps);
      var patchDocument = new JsonPatchDocument
      {
         new JsonPatchOperation
         {
            Operation = Operation.Add,
            Path = "/fields/Microsoft.VSTS.TCM.Steps",
            Value = stepsXml
         }
      };

      var updatedWorkItem = await client.UpdateWorkItemAsync(patchDocument, testCaseId);
      Console.WriteLine($"Test case {testCaseId} updated with new steps.");
   }

   public async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestPoint> GetTestPointForTestCaseAsync(int suiteId, int testCaseId)
   {
      var client = _connection.GetClient<TestManagementHttpClient>();
      var points = await client.GetPointsAsync(
         _projectName,
         _testPlan.Id,
         suiteId,
         testCaseId: testCaseId.ToString(),
         includePointDetails: true
      );

      if (points == null || !points.Any())
      {
         throw new Exception($"No test point found for Test Case ID: {testCaseId}");
      }

      return points.First();
   }

   public async Task DeleteTestPlan(string testPlanName)
   {
      try
      {
         var client = _connection.GetClient<TestPlanHttpClient>();
         var allTestPlans = new List<TestPlan>();
         string continuationToken = null;

         do
         {
            var testPlanList = await client.GetTestPlansAsync(_projectName, owner: "Shaun Bautista", continuationToken: continuationToken, includePlanDetails: true);
            allTestPlans.AddRange(testPlanList);
            continuationToken = testPlanList.ContinuationToken;            
         } while (!string.IsNullOrEmpty(continuationToken));
         {
            var testPlanToDelete = allTestPlans.FirstOrDefault(p => p.Name.Equals(testPlanName, StringComparison.OrdinalIgnoreCase));

            if (testPlanToDelete != null)
            {
               await client.DeleteTestPlanAsync(_projectName, testPlanToDelete.Id);
               Console.WriteLine($"Deleted test plan '{testPlanName}'");
            }
            else
            {
               Console.WriteLine($"Test plan '{testPlanName}' does not exist'");
            }
         }
      }
      catch (Exception e)
      {
         Console.WriteLine(e);
         throw;
      }
   }
   
   private string GenerateStepsXml(List<(string action, string expectedResult)> steps)
   {
      var sb = new StringBuilder();
      var stepId = 1;
      sb.Append("<steps>");

      foreach (var step in steps)
      {
         sb.Append($@"
            <step id='{stepId}' type='ActionStep'>
               <parameterizedString isformatted='true'>{step.action}</parameterizedString>
               <parameterizedString isformatted='true'>{step.expectedResult}</parameterizedString>
            </step>
         ");
         stepId++;
      }
      sb.Append("</steps>");
      return sb.ToString();
   }

   public async Task<TestRun> CreateTestRunAsync(string runName)
   {
      if (_testPlan == null)
      {
         throw new InvalidOperationException("Cannot create a test run without a test plan yet");
      }
      
      var client = _connection.GetClient<TestManagementHttpClient>();
      var runCreateModel = new RunCreateModel(
         name: runName,
         plan: new ShallowReference { Id = _testPlan.Id.ToString() },
         isAutomated: true,
         startedDate: DateTime.UtcNow.ToString("o"),
         state: "InProgress"
      );
      _testRun = await client.CreateTestRunAsync(runCreateModel, _projectName);
      Console.WriteLine($"Created test run for {runName}");
      return _testRun;
   }
   
   public async Task DeleteTestCasesAsync(string userName)
   {
      try
      {
         var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
         var wiql = new Wiql
         {
            Query = $@"
               SELECT [System.Id]
               FROM WorkItems
               WHERE [System.WorkItemType] = 'Test Case'
               AND [System.AssignedTo] = '{userName}'
               AND [System.TeamProject] = '{_projectName}'
            "
         };

         var queryResult = await witClient.QueryByWiqlAsync(wiql, _projectName);

         if (!queryResult.WorkItems.Any())
         {
            Console.WriteLine($"No test runs found for {userName}");
            return;
         }

         var testClient = _connection.GetClient<TestManagementHttpClient>();
         var tasks = new List<Task>();
         
         foreach (var workItemRef in queryResult.WorkItems)
         {
            tasks.Add(testClient.DeleteTestCaseAsync(_projectName, workItemRef.Id));
            Console.WriteLine($"Deleting test case with ID {workItemRef.Id}");
         }

         await Task.WhenAll(tasks);
         tasks.Clear();
         
         Console.WriteLine("All assigned test cases have been deleted.");
      }
      catch (Exception ex)
      {
         Console.WriteLine($"Error deleting test cases: {ex.Message}");
      }
   }

   public async Task<int> AzureDevOpsReporterInit(string testPlanName, TestStep step, string fileName)
   {
      try
      {
         _testPlan = await GetOrCreateTestPlanAsync(testPlanName);
         Console.WriteLine("Successfully set up test plan and test suites.");
         var fileSuite =  await TestSuiteSetupAsync(_testPlan.Id, step.Collection, step.Release, fileName);
         return fileSuite.Id;
      }
      catch (Exception e)
      {
         throw new Exception($"Failed to set up test plan: {e}");
      }
   }

   public async Task<TestPlan> GetOrCreateTestPlanAsync(string testPlanName)
   {
      var client = _connection.GetClient<TestPlanHttpClient>();
      var allTestPlans = new List<TestPlan>();
      string continuationToken = null;

      do
      {
         var testPlanList = await client.GetTestPlansAsync(
            project: _projectName, 
            continuationToken: continuationToken
         );
         allTestPlans.AddRange(testPlanList);
         continuationToken = testPlanList.ContinuationToken;
      } while (!string.IsNullOrEmpty(continuationToken));

      var existingTestPlan = allTestPlans.FirstOrDefault(p => p.Name.Equals(testPlanName, StringComparison.OrdinalIgnoreCase));

      if (existingTestPlan != null)
      {
         Console.WriteLine($"Found existing test plan {existingTestPlan.Name}");
         return existingTestPlan;
      }

      var newTestPlan = new TestPlanCreateParams { Name = testPlanName };
      var createdPlan = await client.CreateTestPlanAsync(newTestPlan, _projectName);
         
      Console.WriteLine($"Created new test plan {createdPlan.Name}");
      return createdPlan;
   }

   public async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite> TestSuiteSetupAsync(int planId, string appName, string releaseNumber, string fileName)
   {
      try
      {
         var client = _connection.GetClient<TestPlanHttpClient>();

         var testType = fileName.Contains("smoke", StringComparison.OrdinalIgnoreCase) ? "Smoke Tests" :
            fileName.Contains("regression", StringComparison.OrdinalIgnoreCase) ? "Regression Tests" :
            "Unassigned Tests";

         var rootSuite = await GetOrCreateRootSuiteAsync();
         var appSuite = await GetOrCreateTestSuiteAsync(appName, rootSuite.Id);
         var releaseSuite = await GetOrCreateTestSuiteAsync(releaseNumber, appSuite.Id);
         var testTypeSuite = await GetOrCreateTestSuiteAsync(testType, releaseSuite.Id);
         var fileSuite = await GetOrCreateTestSuiteAsync(fileName, testTypeSuite.Id);

         async Task<Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite> GetOrCreateTestSuiteAsync(string testSuiteName, int parentSuiteId)
         {
            var suiteClient = _connection.GetClient<TestManagementHttpClient>();
            var suiteCreateModel = new SuiteCreateModel(
               suiteType: TestSuiteType.StaticTestSuite.ToString(),
               name: testSuiteName
            );
            var createdSuite = await suiteClient.CreateTestSuiteAsync(
               suiteCreateModel,
               _projectName,
               planId,
               parentSuiteId,
               userState: null,
               cancellationToken: CancellationToken.None
            );
            return createdSuite.FirstOrDefault();
         }

         async Task<TestSuite> GetOrCreateRootSuiteAsync()
         {
            var suites = await client.GetTestSuitesForPlanAsync(_projectName, planId);

            if (suites.Any())
            {
               var rootSuite = suites.First();
               Console.WriteLine($"Found existing root suite {rootSuite.Name}");
               return rootSuite;
            }

            var suiteCreateParams = new TestSuiteCreateParams
            {
               Name = "Root Suite",
               SuiteType = TestSuiteType.StaticTestSuite,
            };
            var newRootSuite = await client.CreateTestSuiteAsync(
               suiteCreateParams,
               _projectName,
               planId
            );

            Console.WriteLine($"Created new root suite {newRootSuite.Name}");
            return newRootSuite;
         }

         Console.WriteLine("Successfully set up test suites.");
         return fileSuite;
      }
      catch (Exception e)
      {
         throw new Exception($"Failed to set up test suite {e}");
      }
   }
}