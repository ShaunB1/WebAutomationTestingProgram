using AutomationTestingProgram.Core;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IPageFactory
    {
        Task<Page> CreatePage(Context context);
    }

    public class PageFactory : IPageFactory
    {
        private readonly IOptions<PageSettings> _settings;
        private readonly ICustomLoggerProvider _provider;
        private readonly IHubContext<TestHub> _hubContext;


        public PageFactory(IOptions<PageSettings> options, ICustomLoggerProvider provider, IHubContext<TestHub> hubContext)
        {
            _settings = options;
            _provider = provider;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Creates a new Page instance.
        /// </summary>
        /// <param name="context">The context object used to create this instance.</param>
        /// <returns></returns>
        public async Task<Page> CreatePage(Context context)
        {
            Page page = new Page(context, _settings, _provider, _hubContext);
            await page.LogInfo($"Initializing Page Object...");
            return page;
        }
    }
}
