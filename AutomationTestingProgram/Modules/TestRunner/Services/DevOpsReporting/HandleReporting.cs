using AutomationTestingProgram.Core;
using AutomationTestingProgram.Models;
using Humanizer;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using NPOI.HPSF;
using System;


namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class HandleReporting
{
    private readonly HandleTestPlan _testPlanHandler;
    private readonly HandleTestRun _testRunHandler;
    private readonly HandleTestSuite _testSuiteHandler;
    private readonly HandleTestCase _testCaseHandler;
    private readonly HandleTestPoint _testPointHandler;
    private readonly HandleTestResult _testResultHandler;

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

    public async Task SetUpDevOps(Func<string, Task> Log, TestRunObject testRunObject, string environment, string testPlanName)
    {
        await Log("Setting up DevOps reporting...");

        TestPlan? testPlan = null;
        try
        {
            await Log("Locating Test Plan...");
            testPlan = await _testPlanHandler.GetTestPlanByNameAsync(testPlanName);
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
        testPlan = await _testPlanHandler.InitializeTestPlanAsync(testPlanName);
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
        testRunObject.ID = await _testRunHandler.CreateTestRunAsync(testPlan.id, testSuite.id, environment, testPlanName);

        await Log("Linking test results to test case...");
        await _testResultHandler.GetTestResultFromTestCaseIdAsync(testRunObject.TestCases, testRunObject.ID);

        await Log("Setup successful");
    }

    public async Task ReportStepResult(TestRunObject testRun, TestCaseObject testCase, TestStepObject testStep, Exception? e = null)
    {
        await _testResultHandler.UpdateTestStepResultAsync(testRun.ID, testCase.ResultID, testStep, e);
    }

    public async Task ReportCaseResult(TestRunObject testRun, TestCaseObject testCase)
    {
        await _testResultHandler.UpdateTestCaseResultAsync(testRun.ID, testCase.ResultID, testCase);
    }

    public async Task CompleteReport(TestRunObject testRun)
    {
        // complete test run
        await _testRunHandler.SetTestRunStateAsync(testRun);
    }
}