﻿using AutomationTestingProgram.Modules.TestRunnerModule;

using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    public class SaveParameter : WebAction
    {
        public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
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
