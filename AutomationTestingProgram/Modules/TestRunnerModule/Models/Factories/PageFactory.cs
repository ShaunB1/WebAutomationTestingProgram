using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IPageFactory
    {
        Page CreatePage(Context context);
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

        /// <summary>
        /// Creates a new Page instance.
        /// </summary>
        /// <param name="context">The context object used to create this instance.</param>
        /// <returns></returns>
        public Page CreatePage(Context context)
        {
            Page page = new Page(context, _settings, _provider);
            return page;
        }
    }
}
