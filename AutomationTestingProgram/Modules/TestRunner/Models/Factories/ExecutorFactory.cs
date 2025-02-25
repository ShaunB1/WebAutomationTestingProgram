using AutomationTestingProgram.Core.Hubs;
using AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Executor;
using AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;
using AutomationTestingProgram.Modules.TestRunnerModule;
using Microsoft.AspNetCore.SignalR;

namespace AutomationTestingProgram.Modules.TestRunner.Models.Factories
{
    public interface IPlaywrightExecutorFactory
    {
        IPlaywrightExecutor CreateExecutor(Context context);
    }

    public class ExecutorFactory : IPlaywrightExecutorFactory
    {
        private readonly IHubContext<TestHub> _hubContext;
        private readonly IReaderFactory _readerFactory;

        public ExecutorFactory(IReaderFactory readerFactory, IHubContext<TestHub> hubContext)
        {
            _readerFactory = readerFactory;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Creates a new Executor instance.
        /// </summary>
        /// <returns></returns>
        public IPlaywrightExecutor CreateExecutor(Context context)
        {
            return new PlaywrightExecutor(context, _readerFactory, _hubContext);
        }
    }
}
