using Microsoft.AspNetCore.SignalR;
using WebAutomationTestingProgram.Core.Hubs;
using WebAutomationTestingProgram.Modules.TestRunnerV2.Services.Playwright.Executor;
using WebAutomationTestingProgram.Modules.TestRunnerV2.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Factories
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
