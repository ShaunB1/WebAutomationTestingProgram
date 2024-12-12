// Exactly the same as PopulateWebElement

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

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
        {
            // Forward the execution to PopulateWebElement
            return await _populateWebElementAction.ExecuteAsync(page, step, iteration, envVars, saveParams);
        }
    }
}