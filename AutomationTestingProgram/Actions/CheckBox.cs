using AutomationTestingProgram.Modules.TestRunnerModule;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckBox : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        IPage page = pageObject.Instance!;

        await pageObject.LogInfo("Locating checkbox...");


        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);

        await pageObject.LogInfo("Checkbox successfully located");


        var state = step.Value.ToLower();
        var isChecked = await element.IsCheckedAsync();

        switch (state)
        {
            case "on" when !isChecked:
                await element.CheckAsync();
                await pageObject.LogInfo("Checkbox checked");
                return;
            case "off" when isChecked:
                await element.UncheckAsync();
                await pageObject.LogInfo("Checkbox unchecked");
                return;
            case "on" when isChecked:
                await pageObject.LogInfo("Checkbox already checked");
                return;
            case "off" when !isChecked:
                await pageObject.LogInfo("Checkbox already unchecked");
                return;
            default:
                throw new Exception($"Failed to set {step.Object} to {state}");
        }
    }
}