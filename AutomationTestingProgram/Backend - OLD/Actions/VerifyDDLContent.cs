
using System;
using System.Linq;
using System.Threading.Tasks;
using AutomationTestingProgram.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

    /// <summary>
    /// This test step verifies the content of a dropdown list using Playwright.
    /// </summary>
public class VerifyDDLContent : IWebAction
{
    public string Name { get; set; } = "VerifyDDLContent";

    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
            //base.Execute();

         var expectedValues = step.Value.ToLower().Split(';').ToList();

        try
        {
            // Locate the dropdown using XPath
            var locator = step.Object;

            var dropdown = step.Comments == "html id"
            ? page.Locator($"#{locator}")
            : step.Comments == "innertext"
            ? page.Locator($"text={locator}")
                : page.Locator(locator);

            // Get all options in the dropdown
            var options = await dropdown.Locator("option").AllTextContentsAsync();

                // Check if the options match the expected values
            if (expectedValues.All(options.Contains))
                {
                step.RunSuccessful = true;
                step.Actual = $"Successfully verified dropdown content with XPath: {locator}";
                }
            else
                {
                step.RunSuccessful = false;
                step.Actual = "Failure in verifying dropdown content";
                throw new Exception(step.Actual);
                }
            return true;
        }
        catch (Exception ex)
        {
            //Logger.Info("Could not verify dropdown content.");
            //step.RunSuccessful = false;
            //HandleException(ex);
            Console.WriteLine($"Failed to click element {step.Object}: {ex.Message}");
            return false;
        }
    }
}
