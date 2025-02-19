
using System;
using System.Threading.Tasks;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Modules.TestRunnerModule;

using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step verifies the URL value of an element using Playwright.
    /// </summary>
    public class VerifyURLValue : WebAction
    {
        public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
        {
            //base.Execute();

            string expectedURL = step.Value.ToLower();

            try
            {
                IPage page = pageObject.Instance!;

                await pageObject.LogInfo("Verifying url...");
                
                // Locate the element and get its URL (href attribute)
                var locator = step.Object;

                var element = step.Comments == "html id"
                ? page.Locator($"#{locator}")
                : step.Comments == "innertext"
                ? page.Locator($"text={locator}")
                    : page.Locator(locator);
                string actualURL = await element.GetAttributeAsync("href") ?? string.Empty;

                if (actualURL == expectedURL)
                {
                    await pageObject.LogInfo($"Expected URL and Found URL match: {expectedURL}");
                }
                else
                {
                    throw new Exception($"Expected URL ({expectedURL}) does not match Found URL ({actualURL})");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
