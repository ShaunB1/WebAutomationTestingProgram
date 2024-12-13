﻿using Microsoft.Playwright;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;

namespace AutomationTestingProgram.Backend.Actions;

public class ChooseAllDDL : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
    {
        try
        {
            var selectElements = await page.QuerySelectorAllAsync("select");

            foreach (var selectElement in selectElements)
            {
                var isVisible = await selectElement.IsVisibleAsync();
                var isEnabled = await selectElement.IsEnabledAsync();

                if (isVisible && isEnabled)
                {
                    var selectValue = await selectElement.InputValueAsync();
                    if (!string.IsNullOrEmpty(selectValue))
                    {
                        var options = await selectElement.QuerySelectorAllAsync("option");

                        if (options.Count > 0)
                        {
                            var firstOption = await options[0].GetAttributeAsync("value");
                            if (firstOption != null)
                            {
                                await selectElement.SelectOptionAsync(firstOption);
                            }
                        }
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