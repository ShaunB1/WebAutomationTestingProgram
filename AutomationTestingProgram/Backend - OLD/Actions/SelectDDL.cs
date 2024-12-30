using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class SelectDDL : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var locator = step.Object;
        var option = step.Value;
        var element = page.Locator(locator);

        Match match = Regex.Match(step.Value, @"^{(\d+)}$");
        var datapoint = string.Empty;

        if (match.Success)
        {
            var content = match.Groups[1].Value;
            var index = int.Parse(content);
            var datasets = JsonConvert.DeserializeObject<List<List<string>>>(step.Data);
            datapoint = datasets?[iteration][index];
        }
        else
        {
            datapoint = option;
        }
        
        try
        {
            IReadOnlyList<string>? res = null;
            if (match.Success && iteration != -1)
            {
                res = await element.SelectOptionAsync(new SelectOptionValue { Label = datapoint});
            }
            else
            {
                res = await element.SelectOptionAsync(new SelectOptionValue { Label = option });   
            }

            if (res == null || res.Count == 0)
            {
                await element.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while selecting option {option}: {e.Message}");
            return false;
        }
    }
}