using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class SelectDDL : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);
        var option = GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        
        try
        {
            IReadOnlyList<string>? res = null;
            
            res = await element.SelectOptionAsync(new SelectOptionValue { Label = option });

            if (res == null || res.Count == 0)
            {
                await element.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while selecting option {option}: {e.Message}");
            return false;
        }
    }
}