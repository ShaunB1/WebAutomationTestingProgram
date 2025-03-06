using System.Text.RegularExpressions;
using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class PopulateWebElement : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        var locator = step.Object;
        var locatorType = step.Comments;
        var state = step.Value.ToLower();
        var element = await LocateElementAsync(page, locator, locatorType);
        try
        {
            Match match = Regex.Match(step.Value, @"^{(\d+)}$");
            var datapoint = string.Empty;

            if (match.Success)
            {
                var content = match.Groups[1].Value;
                var index = int.Parse(content);
            }

            if (match.Success)
            {
                await element.FillAsync(datapoint);
            }
            else
            {
                await element.FillAsync(step.Value);                
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to populate text box {step.Object} with {step.Value}: {ex.Message}");
            return false;
        }
    }
}