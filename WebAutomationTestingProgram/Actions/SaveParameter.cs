using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions
{
    public class SaveParameter : WebAction
    {
        public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
            Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
            Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration,
            string cycleGroupName)
        {
            string value = step.Value;
            string obj = step.Object;

            if (value == "" || obj == "")
            {
                Console.Write("Incorrect syntax for SaveParameter - Value and Obj must be filled");
                return false;
            }

            saveParams[obj] = value;
            Console.WriteLine($"Successfully updated parameter {obj} to {value}");
            return true;
        }
    }
}