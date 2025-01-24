using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IContextFactory
    {
        Task<Context> CreateContext(Browser browser);
    }

    public class ContextFactory : IContextFactory
    {
        private readonly IOptions<ContextSettings> _settings;
        private readonly ICustomLoggerProvider _provider;
        private readonly IPageFactory _pageFactory;

        public ContextFactory(IOptions<ContextSettings> options, ICustomLoggerProvider provider, IPageFactory pageFactory)
        {
            _settings = options;
            _provider = provider;
            _pageFactory = pageFactory;
        }

        public async Task<Context> CreateContext(Browser browser)
        {
            Context context = new Context(browser, _settings, _provider, _pageFactory);
            await context.InitializeAsync();
            return context;
        }
    }
}
