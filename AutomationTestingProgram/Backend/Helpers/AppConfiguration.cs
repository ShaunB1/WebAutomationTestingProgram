namespace AutomationTestingProgram.Backend
{
    public static class AppConfiguration
    {
        private static IConfiguration _configuration;

        static AppConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false) // reloadOnChange: true -> Monitors files changes for dynamic reloads if needed
                .Build();
        }

        /// <summary>
        /// Retrieves a setting by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T? GetSetting<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }

        /// <summary>
        /// Retrieves a strongly typed setting by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static T GetSection<T>(string sectionName) where T : new()
        {
            var section = _configuration.GetSection(sectionName);
            var settings = new T();
            section.Bind(settings);
            return settings;
        }
    }
}
