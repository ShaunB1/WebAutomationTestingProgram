using AutomationTestingProgram.Core;
using AutomationTestingProgram.Models;
using Humanizer;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using NPOI.HPSF;
using System;


namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

/// <summary>
/// This is the main class used to report to devops.
/// PlaywrightExecutor will call this class to perform various operations,
/// such as reporting test step, case, run results.
/// </summary>
public class HandleReporting
{
    private readonly HandleTestPlan _testPlanHandler;
    private readonly HandleTestRun _testRunHandler;
    private readonly HandleTestSuite _testSuiteHandler;
    private readonly HandleTestCase _testCaseHandler;
    private readonly HandleTestPoint _testPointHandler;
    private readonly HandleTestResult _testResultHandler;

    /// <summary>
    /// Used to limit concurrent operations on Test Plan. This includes:
    /// - Creating Test Plan
    /// - Creating Test Suites
    /// - Linking Test Cases to Test Suites
    /// - Creating Test Run
    /// </summary>
    private SemaphoreSlim _semaphore;

    public HandleReporting(
        HandleTestPlan planHandler,
        HandleTestRun runHandler,
        HandleTestSuite suiteHandler,
        HandleTestCase caseHandler,
        HandleTestPoint pointHandler,
        HandleTestResult resultHandler
        )
    {
        _testPlanHandler = planHandler;
        _testRunHandler = runHandler;
        _testSuiteHandler = suiteHandler;
        _testCaseHandler = caseHandler;
        _testPointHandler = pointHandler;
        _testResultHandler = resultHandler;
    }

    /// <summary>
    /// Sets up basic DevOps functionality. 
    /// - Each machine will be assigned one Test Plan. 
    ///   All test run executions within a machine will report to different 
    ///   suites within this plan.
    ///   This is done as you cannot make changes to the same test plan concurrently
    /// 
    /// </summary>
    /// <param name="Log"></param>
    /// <param name="testRunObject"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task SetUpDevOps(Func<string, Task> Log, TestRunObject testRunObject, ProcessRequest request)
    {
        await Log("Setting up DevOps reporting...");

        TestPlan? testPlan = null;
        try
        {
            await Log("Locating Test Plan...");
            testPlan = await _testPlanHandler.GetTestPlanByNameAsync(request.FileName);
        }
        catch (NoMatchFoundException)
        {
            await Log($"Test Plan not found.");
        }

        if (testPlan != null)
        {


            // delete test cases in test plan
            await Log("Test Plan found. Deleting all associate test cases...");
            await _testCaseHandler.DeleteTestCasesAsync(_testSuiteHandler, testPlan.id);

            
            // delete existing test plan
            await Log("Test Cases deleted. Deleting Test Plan...");
            await _testPlanHandler.DeleteTestPlan(testPlan.id);
            await Log("Test Plan Deleted");
        }

        await Log("Creating Test Plan...");
        testPlan = await _testPlanHandler.InitializeTestPlanAsync(request.FileName);
        testRunObject.PlanID = testPlan.id;

        await Log("Setting up Test Suites...");
        TestSuite testSuite = await _testSuiteHandler.TestSuiteSetupAsync(testPlan.id, "App Name", "Release Number", $"File Name");
        testRunObject.SuiteID = testSuite.id;

        await Log("Setting up Test Cases...");
        foreach (var testCaseObject in testRunObject.TestCases)
        {
            // create test case
            testCaseObject.ID = await _testCaseHandler.CreateTestCaseAsync(testPlan.id, testSuite.id, testCaseObject.Name);

            // add test steps to test case
            await _testCaseHandler.AddTestStepsToTestCaseAsync(testCaseObject.ID, testCaseObject.TestSteps);


            testCaseObject.PointID = await _testPointHandler.GetTestPointFromTestCaseIdAsync(testPlan.id, testSuite.id, testCaseObject.ID);

        }

        await Log("Setting up Test Run...");

        // create test run
        testRunObject.ID = await _testRunHandler.CreateTestRunAsync(testRunObject, request);

        await Log("Linking test results to test case...");
        await _testResultHandler.GetTestResultFromTestCaseIdAsync(testRunObject.TestCases, testRunObject.ID);

        await Log("Setup successful");
    }

    public async Task ReportStepResult(TestRunObject testRun, TestCaseObject testCase, TestStepObject testStep, Exception? e = null)
    {
        await _testResultHandler.UpdateTestStepResultAsync(testRun.ID, testCase, testStep, e);
    }

    public async Task ReportCaseResult(TestRunObject testRun, TestCaseObject testCase, Exception? e = null)
    {
        await _testResultHandler.UpdateTestCaseResultAsync(testRun.ID, testCase, e);
    }

    public async Task AddAttachment(TestRunObject testRun, string comment, string folderPath, string fileName)
    {
        await _testRunHandler.AddAttachment(testRun, comment, folderPath, fileName);
    }

    public async Task CompleteReport(TestRunObject testRun)
    {
        // complete test run
        await _testRunHandler.SetTestRunStateAsync(testRun);
    }
}