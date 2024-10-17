using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class PopulateWebElement : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step)
    {
        var locator = step.Object;
        var state = step.Value.ToLower();
        var element = step.Comments == "html id" 
            ? page.Locator($"#{locator}") 
            : step.Comments == "innertext" 
                ? page.Locator($"text={locator}") 
                : page.Locator(locator);
        
        try
        {
            await element.ClickAsync();
            await element.FillAsync(step.Value);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to populate text box {step.Object} with {step.Value}: {ex.Message}");
            return false;
        }
    }
}