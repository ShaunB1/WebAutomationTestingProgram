using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class UploadFile : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);
        string filePath;
        
        if (step.Value.StartsWith("{") && step.Value.EndsWith("}"))
        {
            filePath = GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        }
        else
        {
            filePath = step.Value;
        }

        try
        {
            await element.SetInputFilesAsync(filePath);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}