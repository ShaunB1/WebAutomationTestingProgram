using Microsoft.Playwright;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using WebAutomationTestingProgram.Actions;
using WebAutomationTestingProgram.Core.Hubs.Services;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Controllers;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.TeamFoundation;

namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Services.AzureReporter;

public class HandleReporting
{
    private readonly string _testRunId;
    private readonly HandleTestPlan _testPlanHandler;
    private readonly HandleTestRun _testRunHandler;
    private readonly HandleTestSuite _testSuiteHandler;
    private readonly HandleTestCase _testCaseHandler;
    private readonly HandleTestPoint _testPointHandler;
    private readonly HandleTestResult _testResultHandler;
    private readonly ILogger<TestController> _logger;
    private readonly SignalRService _hubContext;
    private TestPlan _testPlan;
    private Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite _testSuite;
    private string _planName;
    private string _environment;

    public HandleReporting(ILogger<TestController> logger, SignalRService hubContext, string testRunId)
    {
        _testPlanHandler = new HandleTestPlan();
        _testRunHandler = new HandleTestRun();
        _testSuiteHandler = new HandleTestSuite();
        _testCaseHandler = new HandleTestCase();
        _testPointHandler = new HandleTestPoint();
        _testResultHandler = new HandleTestResult();
        _logger = logger;
        _hubContext = hubContext;
        _testRunId = testRunId;
        _planName = "Test Environment";
        _environment = "TEST_ENVIRONMENT";
    }

    public async Task DeleteTestCasesAsync()
    {
        await _testCaseHandler.DeleteTestCasesAsync("Shaun Bautista");
    }

    public async Task DeleteTestPlanAsync()
    {
        await _testPlanHandler.DeleteTestPlan(_planName);
    }

    public async Task<(List<int>, int, int)> InitializeTestPlanAsync(List<TestStep> testSteps)
    {
        var (testPlan, testSuite) = await _testPlanHandler.InitializeTestPlanAsync(_planName);
        _testPlan = testPlan;
        _testSuite = testSuite;
        var testCases = testSteps.GroupBy(s => s.TestCaseName);
        var testCaseIds = new List<int>();
        var testCaseNames = new List<string>();
        var testPoints = new List<TestPoint>();

        foreach (var testCase in testCases)
        {
            if (string.IsNullOrEmpty(testCase.Key))
            {
                throw new Exception();
            }
            var testCaseId = await _testCaseHandler.CreateTestCaseAsync(testCase.Key);
            testCaseIds.Add(testCaseId);
            testCaseNames.Add(testCase.Key);
        }

        foreach (var testCaseId in testCaseIds)
        {
            await _testSuiteHandler.AddTestCaseToTestSuite(_testPlan.Id, _testSuite.Id, testCaseId);
            
            var testPoint = await _testPointHandler.GetTestPointFromTestCaseIdAsync(_testPlan.Id, _testSuite.Id, testCaseId);
            testPoints.Add(testPoint);
        }

        foreach (var (testCaseGroup, index) in testCases.Select((group, index) => (group, index)))
        {
            // add test steps to test case
            await _testCaseHandler.AddTestStepsToTestCaseAsync(testCaseIds[index], testCaseGroup.ToList());
        }
        
        return (testCaseIds, _testPlan.Id, _testSuite.Id);
    }

    public async Task<int> CreateTestRunAsync()
    {
        var testRun = await _testRunHandler.CreateTestRunAsync(_testPlan.Id, _testSuite.Id, _environment, _planName);
        return testRun.Id;
    }
    
    public async Task ReportToDevOps(IBrowser browser, List<TestStep> testSteps, string environment, string fileName,
        HttpResponse response, Dictionary<string, List<Dictionary<string, string>>> cycleGroups)
    {
        var testPlanName = fileName;
        var testPlanId = await _testPlanHandler.GetTestPlanIdByNameAsync(testPlanName);
        var testCases = testSteps.GroupBy(s => s.TestCaseName);
        
        if (testPlanId != -1)
        {
            // delete test cases in test plan
            await _testCaseHandler.DeleteTestCasesAsync("Shaun Bautista");
        
            // delete existing test plan
            await _testPlanHandler.DeleteTestPlan(testPlanName);
        }
        
        // set up test plan with test suites
        var (testPlan, testSuite) = await _testPlanHandler.InitializeTestPlanAsync(testPlanName);
        var testCaseIds = new List<int>();
        var testCaseNames = new List<string>();
        var testPoints = new List<TestPoint>();
        var tasks = new List<Task>();
        
        foreach (var testCase in testCases)
        {
            // create test case
            var testCaseId = await _testCaseHandler.CreateTestCaseAsync(testCase.Key);
            testCaseIds.Add(testCaseId);
            testCaseNames.Add(testCase.Key);
        }
        
        _logger.LogInformation($"Created {testCaseIds.Count} work items for {testCaseNames.Count} test cases.");
        // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Created {testCaseIds.Count} work items for {testCaseNames.Count} test cases.");
        
        foreach (var testCaseId in testCaseIds)
        {
            // add test case to test suite
            await _testSuiteHandler.AddTestCaseToTestSuite(testPlan.Id, testSuite.Id, testCaseId);
            
            // add test case test point
            var testPoint = await _testPointHandler.GetTestPointFromTestCaseIdAsync(testPlan.Id, testSuite.Id, testCaseId);
            testPoints.Add(testPoint);
        }
        
        _logger.LogInformation($"Added {testCaseNames.Count} test cases to test suite '{testSuite.Id}'.");
        // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Added {testCaseNames.Count} test cases to test suite '{testSuite.Id}'.");

        // add test steps to test case
        foreach (var (testCaseGroup, index) in testCases.Select((group, index) => (group, index)))
        {
            await _testCaseHandler.AddTestStepsToTestCaseAsync(testCaseIds[index], testCaseGroup.ToList());
            _logger.LogInformation($"Added {testCaseGroup.ToList().Count} test steps to test case '{testCaseGroup.Key}'");
            // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Added {testCaseGroup.ToList().Count} test steps to test case '{testCaseGroup.Key}'");
        }
        
        // create test run
        var testRun = await _testRunHandler.CreateTestRunAsync(testPlan.Id, testSuite.Id, environment, fileName);
        _logger.LogInformation($"Created Test Run '{testRun.Id}'");
        // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Created Test Run '{testRun.Id}'");

        // execute test steps
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var testResults = new List<TestCaseResultParams>();
        var testExecutor = new TestExecutor(_hubContext, _testRunId);
        
        foreach (var (testCase, index) in testCases.Select((tc, index) => (tc, index)))
        {
            var testResultObj = new TestCaseResultParams();
            testResultObj.testCaseId = testCaseIds[index];
            testResultObj.testCaseName = testCase.Key;
            testResultObj.startedDate = DateTime.UtcNow;

            var testPoint = await _testPointHandler.GetTestPointFromTestCaseIdAsync(testPlan.Id, testSuite.Id, testCaseIds[index]);
            var (testCaseResult, failedTests, stackTrace) = await testExecutor.ExecuteTestStepsAsync(page, testCase.ToList(), response, 3, cycleGroups);
            
            testResultObj.testPointId = testPoint.Id;
            testResultObj.completedDate = DateTime.UtcNow;
            testResultObj.outcome = testCaseResult;
            testResultObj.state = "Completed";
            testResultObj.duration = Convert.ToInt32((testResultObj.completedDate - testResultObj.startedDate).TotalMilliseconds);
            testResultObj.errorMsg = testResultObj.outcome == "Failed" ? $"[{failedTests.Count()} STEPS FAILED]\n{string.Join("\n", failedTests.Select(ft => $" {ft.Item1}: {ft.Item2}"))}" : null;
            testResultObj.stackTrace = testResultObj.outcome == "Failed" ? $"{string.Join("\n", stackTrace.Select(st => $"{st.Item1}: {st.Item2}"))}" : null;
            
            testResults.Add(testResultObj);
            failedTests.Clear();
            
            _logger.LogInformation($"Test case '{testCase.Key}' execution completed with outcome: {testCaseResult}");
            // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Test case '{testCase.Key}' execution completed with outcome: {testCaseResult}");
        }

        // add test result to test run
        await _testResultHandler.UpdateTestResultsAsync(testResults, testRun.Id);
        _logger.LogInformation($"Updated test results in test run.");
        // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Updated test results in test run.");
        
        // complete test run
        await _testRunHandler.SetTestRunStateAsync(testRun.Id);
        _logger.LogInformation($"Completed test run {testRun.Id}");
        // await _hubContext.Clients.Group(_testRunId).SendAsync("BroadcastLog", _testRunId, $"Completed test run {testRun.Id}");
    }
}