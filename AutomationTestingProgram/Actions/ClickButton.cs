
using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using NPOI.OpenXmlFormats.Dml;

namespace AutomationTestingProgram.Actions;

public class ClickButton : WebAction
{
    public override async Task<bool> ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        IPage page = pageObject.Instance!;
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);
        try
        {
            await element.ClickAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}
