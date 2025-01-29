using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IPlaywrightExecutorFactory
    {
        Task<Context> CreateContext(Browser browser, string filePath);
    }

    public class Executor : IContextFactory
    {
        private readonly IOptions<ContextSettings> _settings;
        private readonly ICustomLoggerProvider _provider;
        private readonly TestExecutor _executor;
        private readonly IReaderFactory _readerFactory;
        private readonly IPageFactory _pageFactory;

        public Executor(IOptions<ContextSettings> options, ICustomLoggerProvider provider, TestExecutor executor, IReaderFactory readerFactory, IPageFactory pageFactory)
        {
            _settings = options;
            _provider = provider;
            _executor = executor;
            _readerFactory = readerFactory;
            _pageFactory = pageFactory;
        }

        /// <summary>
        /// Creates a new Context instance.
        /// </summary>
        /// <param name="browser">The browser object used to create this instance</param>
        /// <param name="filePath">The filepath of the file to execute with playwright</param>
        /// <returns></returns>
        public async Task<Context> CreateContext(Browser browser, string filePath)
        {
            Context context = new Context(browser, filePath, _settings, _provider, _executor, _readerFactory, _pageFactory);
            await context.InitializeAsync();
            return context;
        }
    }
}
