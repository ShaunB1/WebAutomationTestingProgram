using System.Text.RegularExpressions;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class UploadFile : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        IPage page = pageObject.Instance!;

        await pageObject.LogInfo("Locating upload...");
        
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);

        await pageObject.LogInfo("Upload successfully located");

        string filePath;

        filePath = step.Value;

        try
        {
            await pageObject.LogInfo("Uploading...");

            await element.SetInputFilesAsync(filePath);
            await pageObject.LogInfo("Upload successful");
        }
        catch (Exception e)
        {
            throw;
        }
    }
}