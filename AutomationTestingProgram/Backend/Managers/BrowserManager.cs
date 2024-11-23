using AutomationTestingProgram.Models.Backend;
using AutomationTestingProgram.Services.Logging;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend.Managers
{
    public class BrowserManager
    {        
        private readonly IPlaywright Playwright;
        private readonly SemaphoreSlim BrowserSemaphore;
        private readonly ConcurrentQueue<Func<Task<Browser>>> BrowserQueue;
        private readonly ILogger<BrowserManager> Logger;
        private int BrowserCount;

        public BrowserManager(IPlaywright playwright)
        {
            Playwright = playwright;
            BrowserSemaphore = new SemaphoreSlim(3);
            BrowserQueue = new ConcurrentQueue<Func<Task<Browser>>>();
            BrowserCount = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
            Logger = provider.CreateLogger<BrowserManager>()!;
        }

        public async Task<Browser> CreateNewBrowserAsync(string Type, int Version)
        {
            var browserTask = new TaskCompletionSource<Browser>();

            Func<Task<Browser>> createBrowser = async () =>
            {
                Browser browser = new Browser(Playwright, Type, Version);
                try
                {
                    
                    IncrementBrowserCount(browser);
                    await browser.InitializeAsync();
                    await ExecuteBrowserAsync(browser);
                    browserTask.SetResult(browser);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Run-Level Error encountered\n {e}");
                    browserTask.SetException(e);
                }
                finally
                {
                    DecrementBrowserCount(browser);
                    BrowserSemaphore.Release();
                    await ProcessNextBrowser();
                }
                return await browserTask.Task;
            };

            BrowserQueue.Enqueue(createBrowser);
            await ProcessNextBrowser();
            return await browserTask.Task;
        }

        private async Task ExecuteBrowserAsync(Browser browser)
        {
            try
            {
                var tasks = new[]
                {
                    browser.CreateAndRunContextAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                await browser.CloseAsync();
            }
        }

        private void IncrementBrowserCount(Browser browser)
        {
            Interlocked.Increment(ref BrowserCount);
            Logger.LogInformation($"Executing Browser (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}) -- Browsers Running: '{BrowserCount}' | Queued: '{BrowserQueue.Count}'");
        }

        private void DecrementBrowserCount(Browser browser)
        {
            Interlocked.Decrement(ref BrowserCount);
            Logger.LogInformation($"Terminating Browser (ID: {browser?.ID}, Type: {browser?.Type}, Version: {browser?.Version}) -- Browsers Running: '{BrowserCount}' | Queued: '{BrowserQueue.Count}'");
            // Highest level flushed placed differently compared to others
            if (Logger is CustomLogger<BrowserManager> customLogger)
            {
                customLogger.Flush();
            }
        }

        public async Task ProcessNextBrowser()
        {
            if (await BrowserSemaphore.WaitAsync(0) && BrowserQueue.TryDequeue(out var nextTask))
            {
                _ = Task.Run(nextTask); // DO NOT AWAIT
            }
            else if (BrowserQueue.Count > 0)
            {
                Logger.LogInformation($"Queuing Browser -- Browsers Running: '{BrowserCount}' | Queued: '{BrowserQueue.Count}'");
            }
        }
    }
}
