using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IPageFactory
    {
        Task<Page> CreatePage(Context context);
    }

    public class ContextFactory : IContextFactory
    {
        private readonly IOptions<ContextSettings> _settings;
        private readonly ICustomLoggerProvider _provider;

        public ContextFactory(IOptions<ContextSettings> options, ICustomLoggerProvider provider)
        {
            _settings = options;
            _provider = provider;
        }

        public async Task<Context> CreateContext(Browser browser)
        {
            Context context = new Context(browser, _settings, _provider);
            await context.InitializeAsync();
            return context;
        }
    }
}
