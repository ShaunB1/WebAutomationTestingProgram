﻿using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace AutomationTestingProgram.Services;

public class HandleTestPoint : AzureReporter
{
    public HandleTestPoint() : base() {}

    public async Task<TestPoint> GetTestPointFromTestCaseIdAsync(int testPlanId, int testSuiteId, int testCaseId)
    {
        var testPoints = await _managementClient.GetPointsAsync(_projectName, testPlanId, testSuiteId);
        var testPoint = testPoints.FirstOrDefault(tp => tp.TestCase.Id == testCaseId.ToString());
        Console.WriteLine($"Test Case: {testCaseId}, Test Point Count: {testPoints.Count(tp => tp.TestCase.Id == testCaseId.ToString())}");
        return testPoint;
    }
}