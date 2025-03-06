using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class CheckBox : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);
        
        var state = step.Value.ToLower();
        var isChecked = await element.IsCheckedAsync();
        GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        switch (state)
        {
            case "on" when !isChecked:
                await element.CheckAsync();
                return true;
            case "off" when isChecked:
                await element.UncheckAsync();
                return true;
            case "on" when isChecked:
                return true;
            case "off" when !isChecked:
                return true;
            default:
                Console.WriteLine($"Failed to set {step.Object} to {state}");
                return false;
        }
    }
}