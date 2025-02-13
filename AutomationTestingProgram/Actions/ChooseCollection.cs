using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions;

public class ChooseCollection : WebAction
{
    public override async Task<bool> ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        IPage page = pageObject.Instance!;
        var collection = step.Object;
        var element = await LocateElementAsync(page, "selectedCollection", "html id");
        var url = page.Url;
        try
        {

            await element.ClickAsync();
            await page.GetByText(collection).ClickAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while selecting collection {collection}: {ex.Message}");
            return false;
        }
    }
}


