// Exactly the same as Click WebElement

using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class ClickLink : IWebAction
    {
        public string Name { get; set; } = "ClickLink";

        private readonly ClickWebElement _clickWebElementAction;

        public ClickLink()
        {
            _clickWebElementAction = new ClickWebElement();
        }

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            // Forward the execution to ClickWebElement
            return await _clickWebElementAction.ExecuteAsync(page, step, iteration);
        }
    }
}

/*using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class ClickLink : IWebAction
    {
        public string LinkText { get; set; }
        public string CssSelector { get; set; }

        public ClickLink(string linkText = null, string cssSelector = null)
        {
            LinkText = linkText;
            CssSelector = cssSelector;
        }

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            try
            {
                // Validate that at least one selector is provided
                if (string.IsNullOrEmpty(LinkText) && string.IsNullOrEmpty(CssSelector))
                {
                    throw new ArgumentException("Either LinkText or CssSelector must be provided.");
                }

                // If LinkText is provided, attempt to click by link text
                if (!string.IsNullOrEmpty(LinkText))
                {
                    var link = page.Locator($"text={LinkText}");
                    if (link != null)
                    {
                        await link.ClickAsync();
                    }
                    else
                    {
                        throw new Exception($"Link with text '{LinkText}' not found.");
                    }
                }
                // If CssSelector is provided, attempt to click by CSS selector
                else if (!string.IsNullOrEmpty(CssSelector))
                {
                    var link = page.Locator(CssSelector);
                    if (await link.CountAsync() > 0)
                    {
                        await link.ClickAsync();
                    }
                    else
                    {
                        throw new Exception($"Link with selector '{CssSelector}' not found.");
                    }
                }

                // Optionally, you can add a verification step to check if the page has navigated correctly or if the action succeeded
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during ClickLink execution: {e.Message}");
                return false;
            }
        }
    }
}


using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class ClickLink : IWebAction
    {
        public string LinkText { get; set; }
        public string CssSelector { get; set; }

        public ClickLink(string linkText = null, string cssSelector = null)
        {
            LinkText = linkText;
            CssSelector = cssSelector;
        }

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            try
            {
                // Validate that at least one selector is provided
                if (string.IsNullOrEmpty(LinkText) && string.IsNullOrEmpty(CssSelector))
                {
                    throw new ArgumentException("Either LinkText or CssSelector must be provided.");
                }

                // If LinkText is provided, attempt to click by link text
                if (!string.IsNullOrEmpty(LinkText))
                {
                    var link = page.Locator($"text={LinkText}");

                    // Check if the link is visible and clickable
                    if (await link.CountAsync() > 0)
                    {
                        await link.ClickAsync();
                    }
                    else
                    {
                        throw new Exception($"Link with text '{LinkText}' not found.");
                    }
                }
                // If CssSelector is provided, attempt to click by CSS selector
                else if (!string.IsNullOrEmpty(CssSelector))
                {
                    var link = page.Locator(CssSelector);

                    // Check if the link exists with the CSS selector
                    if (await link.CountAsync() > 0)
                    {
                        await link.ClickAsync();
                    }
                    else
                    {
                        throw new Exception($"Link with selector '{CssSelector}' not found.");
                    }
                }

                // Return true if the click was successful
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during ClickLink execution: {e.Message}");
                return false;
            }
        }
    }
}
*/