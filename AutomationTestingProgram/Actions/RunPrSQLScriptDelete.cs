using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.Extensions.Options;
using NPOI.OpenXmlFormats.Spreadsheet;
using Oracle.ManagedDataAccess.Client;

namespace AutomationTestingProgram.Actions
{
    public class RunPrSQLScriptDelete : WebAction
    {
        private readonly PathSettings _settings;
        private readonly CSVEnvironmentGetter _csvEnvironmentGetter;

        public RunPrSQLScriptDelete(IOptions<PathSettings> options, CSVEnvironmentGetter csvEnvironmentGetter)
        {
            _settings = options.Value;
            _csvEnvironmentGetter = csvEnvironmentGetter;
        }

        public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
        {
            await pageObject.LogInfo($"Changing # of attempts to 1. Executing Picasso Delete...");

            step.LocalAttempts = 1;

            string[] runArgs = step.Object.Split(',');
            string environment = envVars["environment"];

            string text = File.ReadAllText(_settings.Scripts.PR_DELETE);
            text = text.Replace("VARIABLE_OVERWRITE_FORM_NAME", runArgs[0].Trim());
            text = text.Replace("VARIABLE_OVERWRITE_FORM_YEAR", runArgs[1].Trim());
            text = text.Replace("VARIABLE_OVERWRITE_ORG_CODE", runArgs[2].Trim());
            text = text.Replace("VARIABLE_OVERWRITE_DATE_TIME", DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_fff"));

            string Host = _csvEnvironmentGetter.GetHost(environment);
            string Port = _csvEnvironmentGetter.GetPort(environment);
            string DBName = _csvEnvironmentGetter.GetDBName(environment);
            string Username = _csvEnvironmentGetter.GetUsername(environment);
            string Password = _csvEnvironmentGetter.GetPassword(environment);

            string connectionString = $"User Id={Username};Password={Password};Data Source={Host}:{Port}/{DBName}";

            await pageObject.LogInfo($"Connecting to DB...");

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                await pageObject.LogInfo("Connection successful! Executing Delete...");

                using (OracleCommand command = new OracleCommand(text, connection))
                {
                    command.ExecuteNonQuery();                    
                }
            }

            await pageObject.LogInfo("Delete successful!");
        }
    }
}
