
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the content of a web element using Playwright.
    /// </summary>
    public class VerifyWebElementContent : WebAction
    {
        public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
        {
            string expectedContent = step.Value.ToLower();

            try
            {
                IPage page = pageObject.Instance!;

                await pageObject.LogInfo("Locating element...");

                // Locate the element using XPath and verify its text content
                var locator = step.Object;
                var locatorType = step.Comments;
                var element = await LocateElementAsync(page, locator, locatorType);

                await pageObject.LogInfo("Element located");

                string actualContent = await element.InnerTextAsync();

                if (actualContent == expectedContent)
                {
                    await pageObject.LogInfo($"Successfully verified web element content with XPath: {locator}");
                    return;
                }
                else
                {
                    throw new Exception("Failure in verifying web element content");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
