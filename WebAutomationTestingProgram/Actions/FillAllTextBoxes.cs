using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class FillAllTextBoxes : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        try
        {
            var inputElements = await page.QuerySelectorAllAsync("input");
            var text = step.Value;

            foreach (var inputElement in inputElements)
            {
                var isReadOnly = await inputElement.GetAttributeAsync("readonly") != null;
                var isVisible = await inputElement.IsVisibleAsync();
                var isEnabled = await inputElement.IsEnabledAsync();
                
                if (isVisible && isEnabled && !isReadOnly)
                {
                    var inputValue = await inputElement.InputValueAsync();
                
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        await inputElement.FillAsync(text);
                    }
                }
            }
        
            var textAreaElements = await page.QuerySelectorAllAsync("textarea");

            foreach (var textAreaElement in textAreaElements)
            {
                var isReadOnly = await textAreaElement.GetAttributeAsync("readonly") != null;
                var isVisible = await textAreaElement.IsVisibleAsync();
                var isEnabled = await textAreaElement.IsEnabledAsync();

                if (isVisible && isEnabled && !isReadOnly)
                {
                    var textAreaValue = await textAreaElement.InputValueAsync();

                    if (string.IsNullOrEmpty(textAreaValue))
                    {
                        await textAreaElement.FillAsync(text);
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}