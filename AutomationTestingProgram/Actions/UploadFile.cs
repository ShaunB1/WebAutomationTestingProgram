using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Actions;

public class UploadFile : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        var locator = step.Object;
        var filePath = step.Value;
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
                
                await element.SetInputFilesAsync(datapoint);
            }
            else
            {
                await element.SetInputFilesAsync(filePath);                
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