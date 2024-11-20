using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class ClickButton : IWebAction
    {
        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            var locator = step.Object;  // The object to be clicked, typically an ID, XPath, or text
            var waitTime = int.Parse(step.Value);  // Custom wait time from the step data
            var element = step.Comments == "html id"
                ? page.Locator($"#{locator}")
                : step.Comments == "innertext"
                    ? page.Locator($"text={locator}")
                    : page.Locator(locator);  // Locating the element based on the given info

            // Wait if a wait time is specified in the step
            if (waitTime > 0)
            {
                Console.WriteLine($"Sleeping for {waitTime} milliseconds before clicking.");
                await Task.Delay(waitTime);  // Using Task.Delay for async sleep
            }

            try
            {
                // Clicking the element (button)
                await element.ClickAsync();
                Console.WriteLine($"Clicked the button located by '{locator}'.");

                return true;  // Return true if the click was successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to click the button located by '{locator}'. Error: {ex.Message}");
                return false;  // Return false if there was an error
            }
        }
    }
}


/*using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class ClickButton : IWebAction
    {
        // The action name for ClickButton
        public string Name { get; set; } = "ClickButton"; // Update this to reflect the class name

        private readonly ClickWebElement _clickWebElementAction;

        public ClickButton()
        {
            // Create instance of ClickWebElement
            _clickWebElementAction = new ClickWebElement();
        }

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            // Forward the execution to ClickWebElement
            return await _clickWebElementAction.ExecuteAsync(page, step, iteration);
        }
    }
}
*/