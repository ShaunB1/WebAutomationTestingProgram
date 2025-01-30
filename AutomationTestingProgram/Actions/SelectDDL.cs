using System.Text.RegularExpressions;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class SelectDDL : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        IPage page = pageObject.Instance!;

        await pageObject.LogInfo("Locating DDL...");

        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);

        await pageObject.LogInfo("DDL successfully located");


        var option = step.Object;
        
        try
        {
            IReadOnlyList<string>? res = null;
            
            res = await element.SelectOptionAsync(new SelectOptionValue { Label = option });

            if (res == null || res.Count == 0)
            {
                await element.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            }

            await pageObject.LogInfo("DDL selected option successfully");
        }
        catch (Exception e)
        {
            await pageObject.LogError($"Error while selecting option {option}: {e.Message}");
            throw;
        }
    }
}