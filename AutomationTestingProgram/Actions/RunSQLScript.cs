using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text.RegularExpressions;
using AutomationTestingProgram.Actions;
using Microsoft.Playwright;
using Microsoft.PowerShell.Commands;
using Newtonsoft.Json;
using Console = System.Console;

namespace AutomationTestingProgram.Actions;

public class RunSQLScript : WebAction
{
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        var rootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        var scriptPath = GetIterationData(step, cycleGroups, currentIteration, cycleGroupName);
        
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
                Console.WriteLine(results);

                if (ps.HadErrors)
                {
                    foreach (var error in ps.Streams.Error)
                    {
                        Console.WriteLine("Error: " + error.ToString());
                    }

                    return false;
                }

                foreach (var result in results)
                {
                    Console.WriteLine(result.ToString());
                }

                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}