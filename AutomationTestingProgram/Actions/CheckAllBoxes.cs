﻿using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class CheckAllBoxes : IWebAction
{
    public async Task<bool> ExecuteAsync( IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            var checkboxes = await page.QuerySelectorAllAsync("input[type='checkbox']");

            foreach (var checkbox in checkboxes)
            {
                var isVisible = await checkbox.IsVisibleAsync();
                var isEnabled = await checkbox.IsEnabledAsync();

                if (isVisible && isEnabled)
                {
                    var isChecked = await checkbox.IsCheckedAsync();

                    if (!isChecked)
                    {
                        await checkbox.CheckAsync();
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}