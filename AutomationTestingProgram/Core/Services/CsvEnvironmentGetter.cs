using System.Text;
using AutomationTestingProgram.Core.Settings;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Core.Services;

/// <summary>
/// Class to get environment information from environment_list.csv
/// </summary>
public class CsvEnvironmentGetter
{
    private const int EnvironmentNameCol = 0;
    private const int HostCol = 1;
    private const int PortCol = 2;
    private const int DbNameCol = 3;
    private const int UsernameCol = 4;
    private const int PasswordCol = 5;
    private const int UrlCol = 6;
    private const int Url2Col = 7;
    private const int EmailNotificationFolderCol = 8;
    private const int AppInetpubLogCol = 9;
    private const int WebTierLogCol = 10;
    private const int AppServiceLogCol = 11;
    private const int IsEncryptedCol = 12;
    private const int DbTypeCol = 13;
    private const int AppTypeCol = 14;

    private readonly string _fileName;

    public CsvEnvironmentGetter(IOptions<PathSettings> options)
    {
        _fileName = options.Value.EnvironmentsListPath;
    }

    /// <summary>
    /// Verifies that the given environment exists/is valid.
    /// </summary>
    /// <param name="env">The environment to verify</param>
    /// <returns></returns>
    public string GetEnvironmentName(string env)
    {
        if (string.IsNullOrWhiteSpace(env))
        {
            throw new Exception("Environment cannot be empty.");
        }
        
        try
        {
            return GetColumnValue(env, EnvironmentNameCol).Trim();
        }
        catch
        {
            throw new Exception($"Failed to get environment name for '{env}'");
        }
    }

    /// <summary>
    /// Tries to grab the OPS BPS Environment URL from the environment url csv file.
    /// </summary>
    /// <returns>The provided URL for the environment given.</returns>
    public string GetOpsBpsUrl(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, UrlCol).Trim();
        }
        catch
        {
            throw new Exception($"Failed to retrieve OPS BPS URL for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to grab the AAD Environment URL from the environment url csv file.
    /// </summary>
    /// <returns>The provided URL for the environment given.</returns>
    public string GetAadUrl(string env)
    {
        var environmentName = GetEnvironmentName(env);

        try
        {
            return GetColumnValue(env, Url2Col);
        }
        catch
        {
            throw new Exception($"Failed to get AAD URL for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to grab the host from the environment csv file.
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetHost(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, HostCol);
        }
        catch
        {
            throw new Exception($"Failed to retrieve host for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to grab the port from the environment url csv file.
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetPort(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, PortCol);
        }
        catch
        {
            throw new Exception($"Failed to retrieve port for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to determine if encrypted from the environment list csv file.
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetIsEncrypted(string env)
    {
        var environmentName = GetEnvironmentName(env);

        try
        {
            return GetColumnValue(env, IsEncryptedCol);
        }
        catch
        {
            throw new Exception($"Failed to get encryption status for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to grab the username from the environment url csv file.
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetUsername(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, UsernameCol);
        }
        catch
        {
            throw new Exception($"Failed to retrieve username for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to grab the db name from the environment url csv file.
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetDbName(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, DbNameCol);
        }
        catch
        {
            throw new Exception($"Failed to get database name for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// Tries to grab the password from the environment url csv file.
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetPassword(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, PasswordCol);
        }
        catch
        {
            throw new Exception($"Failed to retrieve password for environment '{environmentName}'");
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <returns>The provided password for the environment given.</returns>
    public string GetApplicationType(string env)
    {
        var environmentName = GetEnvironmentName(env);
        
        try
        {
            return GetColumnValue(env, AppTypeCol);
        }
        catch
        {
            throw new Exception($"Failed to retrieve application type for environment '{environmentName}'");
        }
    }

    public List<string> GetEnvironments()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _fileName);

        if (!File.Exists(filePath))
        {
            throw new Exception("environment_list.csv not found!");
        }

        var environments = new List<string>();

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(fileStream, Encoding.UTF8);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',').ToList();

            if (values.Count > EnvironmentNameCol)
            {
                var environmentName = values[EnvironmentNameCol].Trim();
                if (!string.IsNullOrEmpty(environmentName))
                {
                    environments.Add(environmentName);
                }
            }
        }
        
        return environments;
    }
    
    /// <summary>
    /// Returns the column value for the given environment name and column.
    /// </summary>
    /// <returns>The provided column for the environment.</returns>
    private string GetColumnValue(string environment, int columnIndex)
    {
        if (columnIndex is < 0 or > AppTypeCol)
        {
            throw new Exception($"Column index is out of range [0-{AppTypeCol}].");
        }
        
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _fileName);

        if (!File.Exists(filePath))
        {
            throw new Exception("environment_list.csv not found!");
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',').ToList();

            if (values.Count > columnIndex)
            {
                if (values[0].Trim().ToLowerInvariant() == environment.ToLowerInvariant())
                {
                    return values[columnIndex];
                }
            }
        }

        throw new Exception($"Value (ENV: {environment}, COL: {columnIndex}) not found in CsvEnvironmentGetter!");
    }
}
