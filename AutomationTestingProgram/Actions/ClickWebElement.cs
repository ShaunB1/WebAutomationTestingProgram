using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

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
            var variableName = "ORG";
            var pattern = $@"{{{variableName}}}";
            var datapoint = string.Empty;
            
            Console.WriteLine($"CONTAINS PATTERN: {locator.Contains(pattern)}, {pattern}");
            if (locator.Contains(pattern))
            {
                var datasets = JsonConvert.DeserializeObject<List<List<string>>>(step.CycleData);
                datapoint = datasets?[iteration][0];
                locator = locator.Replace(pattern, datapoint);
                
                var newElement = step.Comments == "html id" 
                    ? page.Locator($"#{locator}") 
                    : step.Comments == "innertext" 
                        ? page.Locator($"text={locator}") 
                        : page.Locator(locator);
                Console.WriteLine($"Clicking on {locator}");
                await newElement.ClickAsync();
            }
            else
            {
                await element.ClickAsync();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to click element {step.Object}: {ex.Message}");
            return false;
        }
    }
}