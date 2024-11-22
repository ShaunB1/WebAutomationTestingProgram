using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public class FillAllTextBoxes : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
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