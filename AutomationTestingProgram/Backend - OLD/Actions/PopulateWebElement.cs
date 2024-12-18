using System.Text.RegularExpressions;
using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.Playwright;
using Newtonsoft.Json;
using AutomationTestingProgram.ModelsOLD;

namespace AutomationTestingProgram.Backend.Actions;

public class PopulateWebElement : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStepV1 step, int iteration)
    {
        var locator = step.Object;
        var state = step.Value.ToLower();
        var element = step.Comments == "html id"
            ? page.Locator($"#{locator}")
            : step.Comments == "innertext"
                ? page.Locator($"text={locator}")
                : page.Locator(locator);

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