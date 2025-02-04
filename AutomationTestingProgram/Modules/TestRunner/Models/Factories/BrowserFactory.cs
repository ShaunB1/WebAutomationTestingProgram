using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IBrowserFactory
    {
        Task<Browser> CreateBrowser(PlaywrightObject playwright, string type, string version);
    }
    
    public class BrowserFactory : IBrowserFactory
    {
        private readonly IOptions<BrowserSettings> _settings;
        private readonly ICustomLoggerProvider _provider;
        private readonly IContextFactory _contextFactory;

        public BrowserFactory(IOptions<BrowserSettings> options, ICustomLoggerProvider provider, IContextFactory contextFactory)
        {
            _settings = options;
            _provider = provider;
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Creates a new browser instance.
        /// </summary>
        /// <param name="playwright">The playwrightObject used to create this instance.</param>
        /// <param name="type">The type of browser</param>
        /// <param name="version">The browser version</param>
        /// <returns></returns>
        public async Task<Browser> CreateBrowser (PlaywrightObject playwright, string type, string version)
        {
            Browser browser = new Browser(playwright, type, version, _settings, _provider, _contextFactory);
            await browser.InitializeAsync();
            return browser;
        }
    }
}
