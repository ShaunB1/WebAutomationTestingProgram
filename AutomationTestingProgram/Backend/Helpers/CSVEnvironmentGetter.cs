namespace AutomationTestingProgram.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutomationTestingProgram.Backend;
    using NPOI.SS.UserModel;
    using System.Text;

    /// <summary>
    /// Class to get environment information from environment_list.csv
    /// </summary>
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
        private const int APP_SERVICE_LOG_COL = 11;
        private const int IS_ENCRYPTED_COL = 12;
        private const int DB_TYPE_COL = 13;
        private const int APP_TYPE_COL = 14;
        
        /// <summary>
        /// Verifies that the given environment exists/is valid.
        /// </summary>
        /// <param name="env">The environment to verify</param>
        /// <returns></returns>
        public static async Task<string> GetEnvironmentName(string env)
        {
            return await GetColumnValue(env, ENVIRONMENT_NAME_COL);
        }

        /// <summary>
        /// Tries to grab the OPS BPS Environment URL from the environment url csv file.
        /// </summary>
        /// <returns>The provided URL for the environment given.</returns>
        public static async Task<string> GetOpsBpsURL(string env)
        {
            return await GetColumnValue(env, URL_COL);
        }

        /// <summary>
        /// Tries to grab the AAD Environment URL from the environment url csv file.
        /// </summary>
        /// <returns>The provided URL for the environment given.</returns>
        public static async Task<string> GetAdURL(string env)
        {
            return await GetColumnValue(env, URL2_COL);
        }

        /// <summary>
        /// Tries to grab the host from the environment csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetHost(string env)
        {
            return await GetColumnValue(env, HOST_COL);
        }

        /// <summary>
        /// Tries to grab the port from the environment url csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetPort(string env)
        {
            return await GetColumnValue(env, PORT_COL);
        }

        /// <summary>
        /// Tries to determine if encrypted from the environment list csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetIsEncrypted(string env)
        {
            return await GetColumnValue(env, IS_ENCRYPTED_COL);
        }

        /// <summary>
        /// Tries to grab the username from the environment url csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetUsername(string env)
        {
            return await GetColumnValue(env, USERNAME_COL);
        }

        /// <summary>
        /// Tries to grab the db name from the environment url csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetDBName(string env)
        {
            return await GetColumnValue(env, DB_NAME_COL);
        }

        /// <summary>
        /// Tries to grab the passowrd from the environment url csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetPassword(string env)
        {
            return await GetColumnValue(env, PASSWORD_COL);
        }

        /// <summary>
        /// Tries to grab the passowrd from the environment url csv file.
        /// </summary>
        /// <returns>The provided password for the environment given.</returns>
        public static async Task<string> GetApplicationType(string env)
        {
            return await GetColumnValue(env, APP_TYPE_COL);
        }        

        /// <summary>
        /// Returns the column value for the given environment name and column.
        /// </summary>
        /// <returns>The provided column for the environment.</returns>
        private static async Task<string> GetColumnValue(string environment, int columnIndex)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "environment_list.csv");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("ENVIRONMENT_LIST.CSV FILE NOT FOUND! INVESTIGATE!!");
                return string.Empty;
            }

            try
            {
                // Wait until allowed to read from file
                await IOManager.TryAquireSlotAsync();

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
                {
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
                    }
                }

                throw new Exception($"Value (ENV: {environment}, COL: {columnIndex}) not found in CSVEnvironmentGetter!");
            }
            finally
            {
                IOManager.ReleaseSlot();
            }
        }
    }
}
