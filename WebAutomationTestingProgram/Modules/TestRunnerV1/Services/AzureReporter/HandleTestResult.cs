﻿using Microsoft.TeamFoundation.TestManagement.WebApi;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Services.AzureReporter;

public class HandleTestResult : AzureReporter
{
    public async Task UpdateTestResultsAsync(List<TestCaseResultParams> testResults, int testRunId)
    {
        var existingResults = await _managementClient.GetTestResultsAsync(_projectName, testRunId);
        var testResultsArray = testResults.ToArray();

        foreach (var (testCaseResult, index) in existingResults.Select((result, index) => (result, index)))
        {
            testCaseResult.TestCase = new ShallowReference { Id = testResultsArray[index].testCaseId.ToString() };
            testCaseResult.TestCaseTitle = testResultsArray[index].testCaseName;
            testCaseResult.TestPoint = new ShallowReference { Id = testResultsArray[index].testPointId.ToString() };
            testCaseResult.TestCaseRevision = testResultsArray[index].testCaseRevision;
            testCaseResult.Outcome = testResultsArray[index].outcome;
            testCaseResult.State = testResultsArray[index].state;
            testCaseResult.StartedDate = testResultsArray[index].startedDate;
            testCaseResult.CompletedDate = testResultsArray[index].completedDate;
            testCaseResult.ErrorMessage = testResultsArray[index].errorMsg;
            testCaseResult.StackTrace = testResultsArray[index].stackTrace;
        }

        var updatedResults = await _managementClient.UpdateTestResultsAsync(existingResults.ToArray(), _projectName, testRunId);
    }
}