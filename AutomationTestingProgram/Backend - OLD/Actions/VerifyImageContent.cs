
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the content of an image using Playwright.
    /// </summary>
    public class VerifyImageContent : IWebAction
    {
        public string Name { get; set; } = "Verify Image Content";

        public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
        {
            //base.Execute();

            string expectedHTML = step.Value.ToLower();

            try
            {
                // Locate the image using XPath and verify its "outerHTML" attribute
                var locator = step.Object;

                var image = step.Comments == "html id"
                ? page.Locator($"#{locator}")
                : step.Comments == "innertext"
                ? page.Locator($"text={locator}")
                    : page.Locator(locator);
                string actualHTML = await image.GetAttributeAsync("outerHTML");

                if (actualHTML == expectedHTML)
                {
                    step.RunSuccessful = true;
                    step.Actual = $"Successfully verified image content: {expectedHTML}";
                }
                else
                {
                    step.RunSuccessful = false;
                    step.Actual = "Failure in verifying image content";
                    throw new Exception(step.Actual);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Logger.Info("Could not verify image content.");
                //step.RunSuccessful = false;
                //HandleException(ex);
                Console.WriteLine($"Failed to click element {step.Object}: {ex.Message}");
                return false;
            }
        }
    }
}
