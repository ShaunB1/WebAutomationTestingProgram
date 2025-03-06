using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the URL value of an element using Playwright.
    /// </summary>
    public class VerifyURLValue : WebAction
    {
        public string Name { get; set; } = "Verify URL Value";

        public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
            Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
            Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration,
            string cycleGroupName)
        {
            //base.Execute();
            GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
            string expectedURL = step.Value.ToLower();

            try
            {
                // Locate the element and get its URL (href attribute)
                var locator = step.Object;

                var element = step.Comments == "html id"
                ? page.Locator($"#{locator}")
                : step.Comments == "innertext"
                ? page.Locator($"text={locator}")
                    : page.Locator(locator);
                string actualURL = await element.GetAttributeAsync("href");

                if (actualURL == expectedURL)
                {
                    step.RunSuccessful = true;
                    step.Actual = $"Expected URL and Found URL match: {expectedURL}";
                }
                else
                {
                    step.RunSuccessful = false;
                    step.Actual = $"Expected URL ({expectedURL}) does not match Found URL ({actualURL})";
                    throw new Exception(step.Actual);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Logger.Info("Could not verify URL value.");
                //step.RunSuccessful = false;
                //HandleException(ex);
                Console.WriteLine($"Failed to check checkbox status {step.Object}: {ex.Message}");
                return false;
            }
        }
    }
}
