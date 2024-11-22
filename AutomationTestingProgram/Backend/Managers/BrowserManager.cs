using AutomationTestingProgram.Models.Backend;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend.Managers
{
    public class BrowserManager
    {
        private readonly IPlaywright Playwright;
        private readonly SemaphoreSlim BrowserSemaphore;
        private readonly ConcurrentQueue<Func<Task<IBrowser>>> BrowserQueue;
        private readonly ILogger<BrowserManager> Logger;
        private int RunningBrowserCount;
        private int NextBrowserID;

        public BrowserManager(IPlaywright playwright)
        {
            Playwright = playwright;
            BrowserSemaphore = new SemaphoreSlim(3);
            BrowserQueue = new ConcurrentQueue<Func<Task<IBrowser>>>();
            RunningBrowserCount = 0;
            NextBrowserID = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider("Console");
            Logger = provider.CreateLogger<BrowserManager>()!;
        }

        public async Task<IBrowser> CreateNewBrowserAsync()
        {
            var browserTask = new TaskCompletionSource<IBrowser>();

            Func<Task<IBrowser>> createBrowser = async () =>
            {
                try
                {
                    IBrowser browser = await CreateAndRunBrowserAsync();
                    browserTask.SetResult(browser);
                }
                catch (Exception e)
                {
                    browserTask.SetException(e);
                }
                finally
                {
                    BrowserSemaphore.Release();
                    await ProcessNextBrowser();
                }
                return await browserTask.Task;
            };

            BrowserQueue.Enqueue(createBrowser);
            await ProcessNextBrowser(0);
            return await browserTask.Task;
        }

        private async Task<IBrowser> CreateAndRunBrowserAsync()
        {
            IBrowser browser = Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Channel = "chrome"
            }).GetAwaiter().GetResult();
            try
            {
                IncrementBrowserCount();

                int browserID = Interlocked.Increment(ref NextBrowserID);
                string browserFolderPath = "Console";
            }
            catch (Exception e)
            {

            }

        }
    }
}
