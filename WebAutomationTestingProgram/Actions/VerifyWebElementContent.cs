using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the content of a web element using Playwright.
    /// </summary>
    public class VerifyWebElementContent : WebAction
    {
        public string Name { get; set; } = "Verify WebElement Content";

        public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
            Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
            Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration,
            string cycleGroupName)
        {
            //base.Execute();

            string expectedContent = step.Value.ToLower();

            try
            {
                GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
                // Locate the element using XPath and verify its text content
                var locator = step.Object;
                var locatorType = step.Comments;
                var element = await LocateElementAsync(page, locator, locatorType);
                
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