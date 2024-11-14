using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AutomationTestingProgram.Actions
{
    public class ContextManager
    {

        private readonly IBrowser Browser;
        private readonly SemaphoreSlim ContextSemaphore;
        private readonly ConcurrentQueue<Func<Task<IBrowserContext>>> ContextQueue;        
        private readonly ILogger<ContextManager> Logger;
        private readonly ILoggerFactory _loggerFactory;
        private int RunningContextCount;

        private int ContextID;

        public ContextManager(IBrowser browser, ILogger<ContextManager> logger, ILoggerFactory loggerFactory)
        {
            this.Browser = browser;
            this.ContextSemaphore = new SemaphoreSlim(10); // Limit of 10 concurrences contexts.
            this.ContextQueue = new ConcurrentQueue<Func<Task<IBrowserContext>>>();
            this.RunningContextCount = 0;
            this.Logger = logger;
            _loggerFactory = loggerFactory;
            this.ContextID = 0;
        }

        public async Task<IBrowserContext> CreateNewContextAsync()
        {
            var contextTask = new TaskCompletionSource<IBrowserContext>();

            Func<Task<IBrowserContext>> createContext = async () =>
            {
                try
                {
                    IBrowserContext context = await CreateAndRunContextAsync();                    
                    contextTask.SetResult(context);
                }
                catch (Exception e)
                {
                    contextTask.SetException(e);
                }
                finally
                {
                    ContextSemaphore.Release();
                    await ProcessNextContext();
                }
                return await contextTask.Task;
            };

            ContextQueue.Enqueue(createContext);
            await ProcessNextContext();
            return await contextTask.Task;
        }

        private async Task<IBrowserContext> CreateAndRunContextAsync()
        {
            IBrowserContext context = await Browser.NewContextAsync();
            try
            {
                IncrementContextCount();
                await ExecuteContextAsync(context);
                DecrementContextCount();
            }
            catch (Exception e)
            {
                Logger.LogError($"Context execution failed: {e.Message}");
            }

            return context;
        }

        private async Task ExecuteContextAsync(IBrowserContext context)
        {
            try
            { // Use the id here, send it to page manager
                var pageLogger = _loggerFactory.CreateLogger<PageManager>();
                PageManager pageManager = new PageManager(context, ContextID, pageLogger);
                var tasks = new[]
                {
                    pageManager.CreateNewPrimaryPageAsync(),
                    pageManager.CreateNewPrimaryPageAsync(),
                    pageManager.CreateNewPrimaryPageAsync(),
                    pageManager.CreateNewPrimaryPageAsync(),
                    pageManager.CreateNewPrimaryPageAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Logger.LogError($"Context execution failed: {e.Message}");
            }
            finally
            {
                await context.CloseAsync();
            }
        }

        private void IncrementContextCount()
        {
            Interlocked.Increment(ref RunningContextCount);
            Interlocked.Increment(ref ContextID);
            Logger.LogInformation($"Executing Context -- Contexts Running: '{RunningContextCount}' | Queued: '{ContextQueue.Count}'");
        }

        private void DecrementContextCount()
        {
            Interlocked.Decrement(ref RunningContextCount);
            Logger.LogInformation($"Terminating Context -- Contexts Running: '{RunningContextCount}' | Queued: '{ContextQueue.Count}'");
        }

        private async Task ProcessNextContext()
        {
            if (await ContextSemaphore.WaitAsync(0) && ContextQueue.TryDequeue(out var nextTask))
            {
                Task.Run(nextTask); // DO NOT AWAIT
            }
            else if (ContextQueue.Count > 0)
            {
                Logger.LogInformation($"Queuing Context -- Contexts Running: '{RunningContextCount}' | Queued: '{ContextQueue.Count}'");
            }
        }
    }
}
