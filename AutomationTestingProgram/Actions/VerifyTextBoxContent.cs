
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the content of a text box using Playwright.
    /// </summary>
    public class VerifyTextBoxContent : WebAction
    {
        public string Name { get; set; } = "Verify Textbox Content";

        public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
        {
            //base.Execute();

            string expectedValue = step.Value.ToLower();

            try
            {
                // Locate the text box using XPath and verify its "value" attribute
                var locator = step.Object;

                var textBox = step.Comments == "html id"
                ? page.Locator($"#{locator}")
                : step.Comments == "innertext"
                ? page.Locator($"text={locator}")
                    : page.Locator(locator);
                string actualValue = await textBox.InputValueAsync();

                if (actualValue == expectedValue)
                {
                    step.RunSuccessful = true;
                    step.Actual = $"Successfully verified text box content with XPath: {locator}";
                }
                else
                {
                    step.RunSuccessful = false;
                    step.Actual = "Failure in verifying text box content";
                    throw new Exception(step.Actual);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Logger.Info("Could not verify text box content.");
                //step.RunSuccessful = false;
                //HandleException(ex);
                Console.WriteLine($"Failed to check checkbox status {step.Object}: {ex.Message}");
                return false;
            }
        }
    }
}
