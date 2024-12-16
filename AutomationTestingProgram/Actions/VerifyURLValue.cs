
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the URL value of an element using Playwright.
    /// </summary>
    public class VerifyURLValue : IWebAction
    {
        public string Name { get; set; } = "Verify URL Value";

        public async Task<bool> ExecuteAsync(IPage page, TestStep step)
        {
            //base.Execute();

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
