using System.Text.RegularExpressions;
using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class ClickWebElement : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        IPage page = pageObject.Instance!;

        await pageObject.LogInfo("Locating checkbox...");

        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);

        await pageObject.LogInfo("Element successfully located");


        try
        {

            await element.EvaluateAsync("el => el.scrollIntoView()");
            var isVisible = await element.IsVisibleAsync();
            if (!isVisible)
            {
                throw new Exception("Element isn't visible");
            }

            var initialUrl = page.Url;
            
            await element.ClickAsync();
            await pageObject.LogInfo("Element successfully clicked");

            await Task.Delay(1000); // Quick wait to detect change in url

            if (page.Url != initialUrl)
            {
                await pageObject.LogInfo("Change in URL detected. 30 second wait");
                await Task.Delay(30000);
            }
        }
        catch (TimeoutException e)
        {
            throw new Exception($"Couldn't find element: {e}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}