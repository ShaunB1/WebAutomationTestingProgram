using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AutomationTestingProgram.Backend
{
    public class ContextManager
    {

        private readonly IBrowser Browser;
        private readonly SemaphoreSlim ContextSemaphore;
        private readonly ConcurrentQueue<Func<Task<IBrowserContext>>> ContextQueue;
        private readonly ILogger<ContextManager> Logger;
        private int RunningContextCount;
        private int NextContextID;

        public ContextManager(IBrowser browser)
        {
            Browser = browser;
            ContextSemaphore = new SemaphoreSlim(10); // Limit of 10 concurrences contexts.
            ContextQueue = new ConcurrentQueue<Func<Task<IBrowserContext>>>();
            RunningContextCount = 0;
            NextContextID = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
            Logger = provider.CreateLogger<ContextManager>()!;
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

                int contextID = Interlocked.Increment(ref NextContextID);
                string contextFolderPath = LogManager.CreateContextFolder(contextID);
                await ExecuteContextAsync(context, contextID, contextFolderPath);
                DecrementContextCount();
            }
            catch (Exception e)
            {
                Logger.LogError($"Context execution failed: {e.Message}");
            }

            return context;
        }

        private async Task ExecuteContextAsync(IBrowserContext context, int contextID, string contextFolderPath)
        {
            try
            {
                PageManager pageManager = new PageManager(context, contextID, contextFolderPath);
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
