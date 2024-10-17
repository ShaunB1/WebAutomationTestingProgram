using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class SelectDDL : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step)
    {
        var locator = step.Object;
        var option = step.Value;
        var element = page.Locator(locator);
        Console.WriteLine($"EXISTS: {element}");
        try
        {
            Console.WriteLine($"OPTION: {option}");
            var res = await element.SelectOptionAsync(new SelectOptionValue { Label = option });

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