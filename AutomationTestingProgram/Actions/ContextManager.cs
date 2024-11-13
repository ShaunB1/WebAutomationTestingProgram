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

        private IBrowser Browser;
        private SemaphoreSlim ContextSemaphore;
        private ConcurrentQueue<Func<Task<IBrowserContext>>> ContextQueue;
        private int RunningContextCount;
        private readonly ILogger<ContextManager> Logger;

        public ContextManager(IBrowser browser, ILogger<ContextManager> logger)
        {
            this.Browser = browser;
            this.ContextSemaphore = new SemaphoreSlim(10); // Limit of 10 concurrences contexts.
            this.ContextQueue = new ConcurrentQueue<Func<Task<IBrowserContext>>>();
            this.RunningContextCount = 0;
            this.Logger = logger;
        }

        public async Task<IBrowserContext> CreateNewContextAsync()
        {
            var contextTask = new TaskCompletionSource<IBrowserContext>();

            Func<Task<IBrowserContext>> createContext = async () =>
            {
                await ContextSemaphore.WaitAsync();
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
                    ProcessNextContext();
                }
                return contextTask.Task.Result;
            };

            ContextQueue.Enqueue(createContext);
            ProcessNextContext();
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
                Logger.LogInformation($"Context execution failed: {e.Message}");
            }

            return context;
        }

        private async Task ExecuteContextAsync(IBrowserContext context)
        {
            try
            {
                IPage page = await context.NewPageAsync();
                await page.GotoAsync("https://www.google.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://example.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.bing.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.yahoo.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.wikipedia.org");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.reddit.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.microsoft.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.apple.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.amazon.com");
                await Task.Delay(10000);
                await page.GotoAsync("https://www.netflix.com");
                await Task.Delay(10000);
            }
            catch (Exception e)
            {
                
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

        private void ProcessNextContext()
        {
            if (ContextSemaphore.CurrentCount > 0 && ContextQueue.TryDequeue(out var nextTask))
            {
                Task.Run(nextTask);
            }
            else
            {
                Logger.LogInformation($"Queuing Context -- Contexts Running: '{RunningContextCount}' | Queued: '{ContextQueue.Count}'");
            }
        }
    }
}
