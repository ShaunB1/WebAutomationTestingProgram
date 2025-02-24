using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IContextFactory
    {
        Task<Context> CreateContext(Browser browser, ProcessRequest request);
    }

    public class ContextFactory : IContextFactory
    {
        private readonly IOptions<ContextSettings> _settings;
        private readonly ICustomLoggerProvider _provider;
        private readonly IPlaywrightExecutorFactory _executorFactory;
        private readonly IPageFactory _pageFactory;

        public ContextFactory(IOptions<ContextSettings> options, ICustomLoggerProvider provider, IPlaywrightExecutorFactory executorFactory, IPageFactory pageFactory)
        {
            _settings = options;
            _provider = provider;
            _executorFactory = executorFactory;
            _pageFactory = pageFactory;
        }

        /// <summary>
        /// Creates a new Context instance.
        /// </summary>
        /// <param name="browser">The browser object used to create this instance</param>
        /// <param name="request">The request linked with the context</param>
        /// <returns></returns>
        public async Task<Context> CreateContext(Browser browser, ProcessRequest request)
        {
            Context context = new Context(browser, request, _settings, _provider, _pageFactory, _executorFactory);
            await context.InitializeAsync();
            return context;
        }
    }
}
