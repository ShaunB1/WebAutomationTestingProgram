using AutomationTestingProgram.Models;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace AutomationTestingProgram.Actions;

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
    private readonly WebSocketLogBroadcaster _broadcaster;
    
    public HandleReporting(ILogger<TestController> logger, WebSocketLogBroadcaster broadCaster, string testRunId)
    {
        _testPlanHandler = new HandleTestPlan();
        _testRunHandler = new HandleTestRun();
        _testSuiteHandler = new HandleTestSuite();
        _testCaseHandler = new HandleTestCase();
        _testPointHandler = new HandleTestPoint();
        _testResultHandler = new HandleTestResult();
        _logger = logger;
        _broadcaster = broadCaster;
        _testRunId = testRunId;
    }
    
    public async Task ReportToDevOps(IBrowser browser, List<TestStep> testSteps, string environment, string fileName, HttpResponse response)
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
        await _broadcaster.BroadcastLogAsync(
            $"Created {testCaseIds.Count} work items for {testCaseNames.Count} test cases.", _testRunId);
        
        foreach (var testCaseId in testCaseIds)
        {
            // add test case to test suite
            await _testSuiteHandler.AddTestCaseToTestSuite(testPlan.Id, testSuite.Id, testCaseId);
            
            // add test case test point
            var testPoint = await _testPointHandler.GetTestPointFromTestCaseIdAsync(testPlan.Id, testSuite.Id, testCaseId);
            testPoints.Add(testPoint);
        }
        
        _logger.LogInformation($"Added {testCaseNames.Count} test cases to test suite '{testSuite.Id}'.");
        await _broadcaster.BroadcastLogAsync($"Added {testCaseNames.Count} test cases to test suite '{testSuite.Id}'.", _testRunId);

        // add test steps to test case
        foreach (var (testCaseGroup, index) in testCases.Select((group, index) => (group, index)))
        {
            await _testCaseHandler.AddTestStepsToTestCaseAsync(testCaseIds[index], testCaseGroup.ToList());
            _logger.LogInformation($"Added {testCaseGroup.ToList().Count} test steps to test case '{testCaseGroup.Key}'");
            await _broadcaster.BroadcastLogAsync(
                $"Added {testCaseGroup.ToList().Count} test steps to test case '{testCaseGroup.Key}'", _testRunId);
        }
        
        // create test run
        var testRun = await _testRunHandler.CreateTestRunAsync(testPlan.Id, testSuite.Id, environment, fileName);
        _logger.LogInformation($"Created Test Run '{testRun.Id}'");
        await _broadcaster.BroadcastLogAsync($"Created Test Run '{testRun.Id}'", _testRunId);

        // execute test steps
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var testResults = new List<TestCaseResultParams>();
        var testExecutor = new TestExecutor(_logger, _broadcaster, _testRunId);
        
        foreach (var (testCase, index) in testCases.Select((tc, index) => (tc, index)))
        {
            var testResultObj = new TestCaseResultParams();
            testResultObj.testCaseId = testCaseIds[index];
            testResultObj.testCaseName = testCase.Key;
            testResultObj.startedDate = DateTime.UtcNow;

            var testPoint = await _testPointHandler.GetTestPointFromTestCaseIdAsync(testPlan.Id, testSuite.Id, testCaseIds[index]);
            var (testCaseResult, failedTests, stackTrace) = await testExecutor.ExecuteTestStepsAsync(page, testCase.ToList(), response, 3);
            
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
            await _broadcaster.BroadcastLogAsync(
                $"Test case '{testCase.Key}' execution completed with outcome: {testCaseResult}", _testRunId);
        }

        // add test result to test run
        await _testResultHandler.UpdateTestResultsAsync(testResults, testRun.Id);
        _logger.LogInformation($"Updated test results in test run.");
        await _broadcaster.BroadcastLogAsync($"Updated test results in test run.", _testRunId);
        
        // complete test run
        await _testRunHandler.SetTestRunStateAsync(testRun.Id);
        _logger.LogInformation($"Completed test run {testRun.Id}");
        await _broadcaster.BroadcastLogAsync($"Completed test run {testRun.Id}", _testRunId);
    }
}