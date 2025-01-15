
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.OLD.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the content of a web element using Playwright.
    /// </summary>
    public class VerifyWebElementContent : IWebAction
    {
        public string Name { get; set; } = "Verify WebElement Content";

        public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
        {
            //base.Execute();

            string expectedContent = step.Value.ToLower();

            try
            {
                // Locate the element using XPath and verify its text content
                var locator = step.Object;

                var element = step.Comments == "html id"
                ? page.Locator($"#{locator}")
                : step.Comments == "innertext"
                ? page.Locator($"text={locator}")
                    : page.Locator(locator);
                string actualContent = await element.InnerTextAsync();

                if (actualContent == expectedContent)
                {
                    step.RunSuccessful = true;
                    step.Actual = $"Successfully verified web element content with XPath: {locator}";
                }
                else
                {
                    step.RunSuccessful = false;
                    step.Actual = "Failure in verifying web element content";
                    throw new Exception(step.Actual);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check web element {step.Object}: {ex.Message}");
                return false;
            }
        }
    }
}
