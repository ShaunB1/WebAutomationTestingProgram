using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class ClickWebElement : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        var locator = step.Object;
        var locatorType = step.Comments;
        var element = await LocateElementAsync(page, locator, locatorType);
        GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        
        try
        {
            var variableName = "ORG";
            var pattern = $@"{{{variableName}}}";
            
            if (locator.Contains("Continue")) {}
            
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