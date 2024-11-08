using DocumentFormat.OpenXml.Packaging;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckBox : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
    {
        var locator = step.Object;
        var state = step.Value.ToLower();
        var element = step.Comments == "html id" 
            ? page.Locator($"#{locator}") 
            : step.Comments == "innertext" 
                ? page.Locator($"text={locator}") 
                : page.Locator(locator);

        var isChecked = await element.IsCheckedAsync();

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