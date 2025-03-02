using System.Diagnostics;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Playwright;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Actions;

public class RunShellScript : WebAction
{
    public override async Task ExecuteAsync(Page page, string groupID, TestStep step, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        try
        {
            var scriptPath = step.Value;

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            
            process.WaitForExit();

            page.LogInfo($"Script Output: {output}");

            if (!string.IsNullOrEmpty(error))
            {
                page.LogInfo($"Script Error: {error}");
            }
            else
            {
                page.LogInfo($"Script successfully executed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}