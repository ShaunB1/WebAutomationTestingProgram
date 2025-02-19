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

    /* TODO:
     * 
     * 
     * 1. Add blocked. When a run fails or cancels, all the next test cases
     *    should be marked as blocked
     * 2. IMPORTANT: Reuse TestPlan, no deletion. Deleting a test plan deletes a test run,
     *    we can't do this. Instead, we have to do either one of the following:
     *    - Re-use test plans and test cases like Selenium Framework'
     *    - Have one test plan for results. Attempt to move everything under that test plan.
     * 3. Release pipeline. When starting azure devops, also create a release?? Optional
     * 
     * 1. Find Archive (or create it if not found) -> Save its id for quick searching
     * 2. Create test suite per file name, with same test suites in it as before.
     * 3. Add concurrent mechanism. Cannot use same test suite concurrently. Use a set.
     *    throw error
     * 4. If new test suite, create test cases as normal
     * 5. If test suite already exists, traverse through same test suites as above, then
     *    Find the test cases.
     * 6. Compare test cases to current test cases. Test cases should have same name, step, etc.
     *    Hardest step, lots of possiblites: 
     *    Ex: - steps in devops case different from current. 
     *        - case names are differnet, do I delete the test case??
     *        
     *        
     *        
     *        
     *        
     *    
     * 
     * 
     */

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