
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.Actions;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the content of an HTML editor using Playwright.
    /// </summary>
    public class VerifyHTMLEditorContent : WebAction
    {
        public string Name { get; set; } = "Verify HTML Editor Content";

        public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
            Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
            Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration,
            string cycleGroupName)
        {
            //base.Execute();

            string expectedContent = step.Value.ToLower();
            string iframeSelector = step.Object.ToLower();

            try
            {
                // Switch to the iframe containing the editor
                var iframe = page.FrameLocator(iframeSelector);

                // Verify content inside the body of the iframe
                var body = iframe.Locator("body");
                var actualContent = await body.InnerTextAsync();

                if (actualContent.Trim() == expectedContent.Trim())
                {
                    step.RunSuccessful = true;
                    step.Actual = $"Successfully verified HTML editor content in iframe: {iframeSelector}";
                }
                else
                {
                    step.RunSuccessful = false;
                    step.Actual = "Failure in verifying HTML editor content";
                    throw new Exception(step.Actual);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Logger.Info("Could not verify HTML editor content.");
                //step.RunSuccessful = false;
                //HandleException(ex);
                Console.WriteLine($"Failed to check checkbox status {step.Object}: {ex.Message}");
                return false;
            }
        }
    }
}
