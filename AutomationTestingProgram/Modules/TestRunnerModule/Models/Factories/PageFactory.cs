using AutomationTestingProgram.Core;
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

        public PageFactory(IOptions<PageSettings> options, ICustomLoggerProvider provider)
        {
            _settings = options;
            _provider = provider;
        }

        public async Task<Page> CreatePage(Context context)
        {
            Page page = new Page(context, _settings, _provider);
            await page.InitializeAsync();
            return page;
        }
    }
}
