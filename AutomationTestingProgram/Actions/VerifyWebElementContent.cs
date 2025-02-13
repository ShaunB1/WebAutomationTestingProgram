using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class VerifyWebElementContent : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        try
        {
            IPage page = pageObject.Instance;

            await pageObject.LogInfo("Locating element...");

            var locator = step.Object;
            var locatorType = step.Comments;
            var element = await LocateElementAsync(page, locator, locatorType);
        
            await pageObject.LogInfo("Element found.");

            var expectedContent = step.Value;
        
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var actualContent = await element.TextContentAsync();

            if (expectedContent == actualContent)
            {
                await pageObject.LogInfo($"Successfully verified web element content with XPath: {locator}");
            }
            else
            {
                await pageObject.LogInfo($"Unsuccessfully verified web element content with XPath: {locator}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Couldn't find element: {ex}");
        }
    }
}