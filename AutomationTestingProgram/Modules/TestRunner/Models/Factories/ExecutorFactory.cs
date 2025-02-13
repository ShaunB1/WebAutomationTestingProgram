using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Executor;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IPlaywrightExecutorFactory
    {
        IPlaywrightExecutor CreateExecutor(Context context);
    }

    public class ExecutorFactory : IPlaywrightExecutorFactory
    {
        private readonly IHubContext<TestHub> _hubContext;
        private readonly IReaderFactory _readerFactory;
        private readonly HandleReporting _reporter;

        public ExecutorFactory(HandleReporting reporter, IReaderFactory readerFactory, IHubContext<TestHub> hubContext)
        {
            _readerFactory = readerFactory;
            _hubContext = hubContext;
            _reporter = reporter;
        }

        /// <summary>
        /// Creates a new Executor instance.
        /// </summary>
        /// <returns></returns>
        public IPlaywrightExecutor CreateExecutor(Context context)
        {
            return new PlaywrightExecutor(_reporter, context, _readerFactory, _hubContext);
        }
    }
}
