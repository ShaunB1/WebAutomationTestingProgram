using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class ClickWebElement : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var locator = step.Object;
        // var element = step.Comments == "html id" 
        //     ? page.Locator($"#{locator}") 
        //     : step.Comments == "innertext" 
        //         ? page.Locator($"text={locator}") 
        //         : page.Locator(locator);
        var element = await LocateElementAsync(page, locator);
        
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
                await element.EvaluateAsync("el => el.scrollIntoView()");
                var isVisible = await element.IsVisibleAsync();
                if (isVisible)
                {
                    await element.ClickAsync();
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            var xpath = await element.EvaluateAsync<string>(@"
                (el) => {
                    if (!el) return '';
                    let path = '';
                    while (el.nodeType === Node.ELEMENT_NODE) {
                        let count = 0;
                        let sibling = el;
                        while (sibling) {
                            if (sibling.nodeName === el.nodeName) {
                                count++;
                            }
                            sibling = sibling.previousSibling;
                        }
                        let tagName = el.nodeName.toLowerCase();
                        let index = count > 1 ? `[${count}]` : '';
                        path = '/' + tagName + index + path;
                        el = el.parentNode;
                    }
                    return path;
                }
            ");
            Console.WriteLine($"XPath: {xpath}");
            Console.WriteLine($"Failed to click element {step.Object}: {ex.Message}");
            return false;
        }
    }
}