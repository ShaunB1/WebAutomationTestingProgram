using Microsoft.Extensions.Logging.Console;
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class ChooseCollection : WebAction
{
    public override async Task<bool> ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
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


