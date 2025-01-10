﻿using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class ExitCondition : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            var exitCondition = step.Value;
            var locator = step.Object;

            if (!string.IsNullOrEmpty(exitCondition))
            {
                if (exitCondition == "EXISTS")
                {
                    var element = page.Locator(locator);
                    var isVisible = await element.IsVisibleAsync();
                    if (isVisible)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return false;
    }
}