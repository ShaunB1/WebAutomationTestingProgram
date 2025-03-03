namespace WebAutomationTestingProgram.Modules.TestRunnerV1.Services;

public class CSVEnvironmentGetter
{
    private const int ENVIRONMENT_NAME_COL = 0;
    private const int HOST_COL = 1;
    private const int PORT_COL = 2;
    private const int DB_NAME_COL = 3;
    private const int USERNAME_COL = 4;
    private const int PASSWORD_COL = 5;
    private const int URL_COL = 6;
    private const int URL2_COL = 7;
    private const int EMAIL_NOTIFICATION_FOLDER_COL = 8;
    private const int APP_INETPUB_LOG_COL = 9;
    private const int WEB_TIER_LOG_COL = 10;
    private const int APP_TIER_LOG_COL = 11;
    private const int IS_ENCRYPTED_COL = 12;
    private const int DB_TYPE_COL = 13;
    private const int APP_TYPE_COL = 14;
    private static readonly string? _environmentsListPath;

    static CSVEnvironmentGetter()
    {
        _environmentsListPath = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build()["Paths:EnvironmentsListPath"];
    }

    public static string GetOpsBpsURL(string env)
    {
        string url = GetColumnValue(env, URL_COL);

        if (url == string.Empty)
        {
            Console.WriteLine("Could not get OPS BPS url value for env " + env);
        }

        return url;
    }

    public static string GetAdURL(string env)
    {
        try
        {
            string url = GetColumnValue(env, URL2_COL);

            if (url == string.Empty)
            {
                Console.WriteLine("Could not get AAD url value for env " + env);
            }

            return url;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static string GetColumnValue(string environment, int columnIndex)
    {
        try
        {
            string baseDirectory = AppContext.BaseDirectory;
            string filepath = _environmentsListPath.Replace("%PROJECT_ROOT%", baseDirectory);

            if (!File.Exists(filepath))
            {
                Console.WriteLine($"Environments list file at path {filepath} does not exist");
                return string.Empty;
            }

            StreamReader reader = new StreamReader(filepath);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                List<string> values = line.Split(',').ToList();

                if (values.Count > columnIndex)
                {
                    if (values[0].Trim() == environment)
                    {
                        // for debugging purposes
                        // Logger.Info("Value at index " + values[columnIndex]);
                        return values[columnIndex];
                    }
                }
                else
                {
                    // comments from old framework
                    // for debug
                    // Logger.Warn("GetColumnValue() not enough values available to index");
                }
            }

            Console.WriteLine("Could not retrieve value in environments table for env " + environment);
            return string.Empty;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}

