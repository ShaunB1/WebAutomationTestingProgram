
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.Actions;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

/// <summary>
/// This test step verifies the status of a checkbox using Playwright.
/// </summary>
public class VerifyCheckBoxStatus : WebAction
{
    public string Name { get; set; } = "Verify Checkbox Status";

    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        //base.Execute();

        string expectedValue = step.Value.ToLower();

        try
        {
            // Locate the checkbox using XPath
            var locator = step.Object;

            var checkbox = step.Comments == "html id"
            ? page.Locator($"#{locator}")
            : step.Comments == "innertext"
            ? page.Locator($"text={locator}")
                : page.Locator(locator);

            // Verify the state of the checkbox
            bool isChecked = await checkbox.IsCheckedAsync();

            if ((expectedValue == "on" && isChecked) || (expectedValue == "off" && !isChecked))
            {
                step.RunSuccessful = true;
                step.Actual = $"Successfully verified checkbox status with XPath: {locator}";//was this.Xpath
            }
            else
            {
                step.RunSuccessful = false;
                step.Actual = "Failure in verifying checkbox status";
                throw new Exception(step.Actual);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to check checkbox status {step.Object}: {ex.Message}");
            return false;
        }
    }
}
