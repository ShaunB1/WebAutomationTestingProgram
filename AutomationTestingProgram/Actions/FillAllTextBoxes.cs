using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class FillAllTextBoxes : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        try
        {
            IPage page = pageObject.Instance!;

            await pageObject.LogInfo("Locating all inputs...");

            var inputElements = await page.QuerySelectorAllAsync("input");
            var text = step.Value;

            await pageObject.LogInfo($"Inputs successfully located: {inputElements.Count}");

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

            await pageObject.LogInfo("All inputs are filled");


            await pageObject.LogInfo("Locating all textareas...");

            var textAreaElements = await page.QuerySelectorAllAsync("textarea");

            await pageObject.LogInfo($"Textareas successfully located: {textAreaElements.Count}");


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

            await pageObject.LogInfo("All textareas are filled");

        }
        catch (Exception)
        {
            throw;
        }
    }
}