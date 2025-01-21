using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class ClickWebElement : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);

        try
        {
            var variableName = "ORG";
            var pattern = $@"{{{variableName}}}";

            Console.WriteLine($"CONTAINS PATTERN: {locator.Contains(pattern)}, {pattern}");
            if (locator.Contains(pattern))
            {
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
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Couldn't find element: {ex}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}