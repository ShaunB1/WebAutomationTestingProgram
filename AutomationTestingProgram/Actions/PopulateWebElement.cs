using System.Text.RegularExpressions;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class PopulateWebElement : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {

        await pageObject.LogInfo("Locating element...");

        IPage page = pageObject.Instance!;

        var locator = step.Object;
        var locatorType = step.Comments;
        var state = step.Value.ToLower();
        var element = await LocateElementAsync(page, locator, locatorType);

        await pageObject.LogInfo("Element successfully located");

        try
        {
            await element.FillAsync(step.Value);
            await pageObject.LogInfo("Element successfully filled");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to populate text box {step.Object} with {step.Value}: {ex.Message}");
        }
    }
}