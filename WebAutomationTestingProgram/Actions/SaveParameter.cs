using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions
{
    public class SaveParameter : WebAction
    {
        public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
        {
            await pageObject.LogInfo("Saving parameter...");
            
            string value = step.Value;
            string obj = step.Object;

            if (value == "" || obj == "")
            {
                await pageObject.LogInfo("Incorrect syntax for SaveParameter - Value and Obj must be filled");
            }

            saveParams[obj] = value;
            await pageObject.LogInfo($"Successfully updated parameter {obj} to {value}");
        }
    }
}
