using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    public class SaveParameter : IWebAction
    {
        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            return false;
        }
        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> SaveParameters)
        {
            string value = step.Value;
            string obj = step.Object;

            if (value == "" || obj == "")
            {
                Console.Write("Incorrect syntax for SaveParameter - Value and Obj must be filled");
                return false;
            }

            SaveParameters[obj] = value;
            Console.WriteLine($"Successfully updated parameter {obj} to {value}");
            return true;
        }
    }
}
