using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class ClickWebElement : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
    {
        var locator = step.Object;
        var element = step.Comments == "html id" 
            ? page.Locator($"#{locator}") 
            : step.Comments == "innertext" 
                ? page.Locator($"text={locator}") 
                : page.Locator(locator);
        
        try
        {
            await element.ClickAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to click element {step.Object}: {ex.Message}");
            return false;
        }
    }
}