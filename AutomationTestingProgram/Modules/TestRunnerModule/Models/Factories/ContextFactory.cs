﻿using AutomationTestingProgram.Core;
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
        private readonly IPlaywrightExecutor _executor;
        private readonly IPageFactory _pageFactory;

        public ContextFactory(IOptions<ContextSettings> options, ICustomLoggerProvider provider, IPlaywrightExecutor executor, IPageFactory pageFactory)
        {
            _settings = options;
            _provider = provider;
            _executor = executor;
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
            Context context = new Context(browser, request, _settings, _provider, _pageFactory, _executor);
            await context.InitializeAsync();
            return context;
        }
    }
}
