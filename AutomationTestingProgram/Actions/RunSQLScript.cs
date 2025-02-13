using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text.RegularExpressions;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Playwright;
using Microsoft.PowerShell.Commands;
using Newtonsoft.Json;
using Console = System.Console;

namespace AutomationTestingProgram.Actions;

public class RunSQLScript : WebAction
{
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStepObject step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {
        await pageObject.LogInfo($"Changing # of attempts to 1. Executing SQL Script.");


        var rootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        var scriptPath = step.Object;

        try
        {
            var sqlExecutor = Path.Combine(rootPath, "Actions", "execute_sql.ps1");
            var username = "OPS_WRITE";
            var password = "qateamrw1#";
            var hostname = "cscgikdcdbora47.cihs.gov.on.ca";
            var port = "1521";
            var sid = "edcs9";
            
            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddScript("Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force");
                ps.Invoke();
            
                ps.AddCommand(sqlExecutor);
            
                ps.AddParameter("username", username);
                ps.AddParameter("password", password);
                ps.AddParameter("hostname", hostname);
                ps.AddParameter("port", port);
                ps.AddParameter("sid", sid);
                ps.AddParameter("scriptPath", scriptPath);
            
                Console.WriteLine("Executing SQL script...");
                Collection<PSObject> results = ps.Invoke();

                if (ps.HadErrors)
                {
                    foreach (var error in ps.Streams.Error)
                    {
                        await pageObject.LogError("Error: " + error.ToString());
                    }

                    throw new Exception("Errors encountered");
                }

                foreach (var result in results)
                {
                    await pageObject.LogInfo(result.ToString());
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
}