﻿using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;
public class Comment : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        return true;
    }
}