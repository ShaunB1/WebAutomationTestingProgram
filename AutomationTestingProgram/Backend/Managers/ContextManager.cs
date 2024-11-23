using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AutomationTestingProgram.Models.Backend;

namespace AutomationTestingProgram.Backend
{
    public class ContextManager
    {

        private readonly Browser Browser;
        private readonly SemaphoreSlim ContextSemaphore;
        private readonly ConcurrentQueue<Func<Task<Context>>> ContextQueue;
        private readonly ILogger<ContextManager> Logger;
        private int ContextCount;

        public ContextManager(Browser browser)
        {
            Browser = browser;
            ContextSemaphore = new SemaphoreSlim(10); // Limit of 10 concurrences contexts.
            ContextQueue = new ConcurrentQueue<Func<Task<Context>>>();
            ContextCount = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(browser.FolderPath);
            Logger = provider.CreateLogger<ContextManager>()!;
        }

        public async Task<Context> CreateNewContextAsync()
        {
            var contextTask = new TaskCompletionSource<Context>();

            Func<Task<Context>> createContext = async () =>
            {
                Context context = new Context(Browser);
                try
                {
                    IncrementContextCount(context);
                    await context.InitializeAsync();
                    await ExecuteContextAsync(context);
                    contextTask.SetResult(context);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Browser-Level Error encountered\n {e}");
                    contextTask.SetException(e);
                }
                finally
                {
                    DecrementContextCount(context);
                    ContextSemaphore.Release();
                    await ProcessNextContext();
                }
                return await contextTask.Task;
            };

            ContextQueue.Enqueue(createContext);
            await ProcessNextContext();
            return await contextTask.Task;
        }

        private async Task ExecuteContextAsync(Context context)
        {
            try
            {
                var tasks = new[]
                {
                    context.CreateAndRunPageAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                await context.CloseAsync();
            }
        }

        private void IncrementContextCount(Context context)
        {
            Interlocked.Increment(ref ContextCount);
            Logger.LogInformation($"Executing Context (ID: {context.ID}) -- Contexts Running: '{ContextCount}' | Queued: '{ContextQueue.Count}'");
        }

        private void DecrementContextCount(Context context)
        {
            Interlocked.Decrement(ref ContextCount);
            Logger.LogInformation($"Terminating Context (ID: {context.ID}) -- Contexts Running: '{ContextCount}' | Queued: '{ContextQueue.Count}'");
        }

        private async Task ProcessNextContext()
        {
            if (await ContextSemaphore.WaitAsync(0) && ContextQueue.TryDequeue(out var nextTask))
            {
                _ = Task.Run(nextTask); // DO NOT AWAIT
            }
            else if (ContextQueue.Count > 0)
            {
                Logger.LogInformation($"Queuing Context -- Contexts Running: '{ContextCount}' | Queued: '{ContextQueue.Count}'");
            }
        }
    }
}
