﻿using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    public class SaveParameter : WebAction
    {
        public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
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
