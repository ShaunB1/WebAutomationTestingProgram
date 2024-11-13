// Already exists as PopulateWebElement

using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class PopulateTextBox : IWebAction
    {
        public string Name { get; set; } = "PopulateTextBox";

        private readonly PopulateWebElement _populateWebElementAction;

        public PopulateTextBox()
        {
            _populateWebElementAction = new PopulateWebElement();
        }

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            // Forward the execution to PopulateWebElement
            return await _populateWebElementAction.ExecuteAsync(page, step, iteration);
        }
    }
}


/*using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class PopulateTextBox : IWebAction
    {
        public string Name { get; set; } = "PopulateTextBox";

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            var locator = step.Object;  // The locator for the text box (e.g., ID, XPath, etc.)
            var text = step.Value;      // The value to populate in the text box
            var jsCommand = step.Comments; // JavaScript command or other specific criteria (optional)

            Console.WriteLine($"Trying to populate text box with text: {text}");

            try
            {
                // Clear the existing value in the text box and type the new text
                var element = page.Locator(locator);

                // Clear the text box before entering new text
                await element.FillAsync(string.Empty); // Clears the input field
                await element.FillAsync(text);         // Types in the new text

                // Optionally, wait for the page to finish loading or for any loading spinner to disappear
                if (step.Arguments.ContainsKey("waitForLoading"))
                {
                    var waitForLoading = bool.Parse(step.Arguments["waitForLoading"].ToString());
                    if (waitForLoading)
                    {
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    }
                }

                // Verify that the value is correctly populated
                var fieldValue = await element.InputValueAsync();
                if (fieldValue != text)
                {
                    Console.WriteLine($"Failed to populate textbox. Expected: {text}, but got: {fieldValue}");
                    return false;
                }

                // Optionally, verify if the value contains the expected text if full match fails
                if (!fieldValue.Contains(text))
                {
                    Console.WriteLine($"Textbox value doesn't contain expected text. Expected substring: {text}, but got: {fieldValue}");
                    return false;
                }

                Console.WriteLine("Successfully populated the text box.");
                return true; // Success if the value is as expected
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to populate the text box using locator '{locator}'. Error: {ex.Message}");
                return false; // Failure if any exception occurs
            }
        }
    }
}*/