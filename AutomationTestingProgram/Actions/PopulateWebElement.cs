using System.Text.RegularExpressions;
using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class PopulateWebElement : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var locator = step.Object;
        var locatorType = step.Comments;
        var state = step.Value.ToLower();
        var element = await LocateElementAsync(page, locator, locatorType);
        try
        {
            Match match = Regex.Match(step.Value, @"^{(\d+)}$");
            var datapoint = string.Empty;

            if (match.Success)
            {
                var content = match.Groups[1].Value;
                var index = int.Parse(content);
                var datasets = JsonConvert.DeserializeObject<List<List<string>>>(step.Data);
                datapoint = datasets?[iteration][index];
            }

            if (match.Success && iteration != -1)
            {
                await element.FillAsync(datapoint);
            }
            else
            {
                await element.FillAsync(step.Value);                
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to populate text box {step.Object} with {step.Value}: {ex.Message}");
            return false;
        }
    }
}