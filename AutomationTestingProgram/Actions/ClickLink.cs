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

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
        {
            // Forward the execution to ClickWebElement
            return await _clickWebElementAction.ExecuteAsync(page, step, iteration, envVars, saveParams);
        }
    }
}