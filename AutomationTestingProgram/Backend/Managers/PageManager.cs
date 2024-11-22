using AutomationTestingProgram.Services.Logging;
using Microsoft.Identity.Client;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend
{
    public class PageManager
    {
        // If a list of 10+ pages exist (for an active page), close all the pages and active page, log an error!!

        private readonly IBrowserContext Context;
        private readonly int ContextID;
        private readonly string ContextFolderPath;
        private readonly SemaphoreSlim ActivePageSemaphore;
        private readonly ConcurrentQueue<Func<Task<IPage>>> PageRequestQueue;
        private readonly ConcurrentDictionary<IPage, List<IPage>> Pages; // Primary -> Secondarys
        private readonly ILogger<PageManager> Logger;
        private int NextPageID;

        private const int MaxActivePages = 3;

        private int ActivePageCount;
        private int PageCount;

        public PageManager(IBrowserContext context, int contextID, string contextFolderPath)
        {
            Context = context;
            ContextID = contextID;
            ContextFolderPath = contextFolderPath;
            ActivePageSemaphore = new SemaphoreSlim(MaxActivePages);
            PageRequestQueue = new ConcurrentQueue<Func<Task<IPage>>>();
            Pages = new ConcurrentDictionary<IPage, List<IPage>>();
            ActivePageCount = 0;
            PageCount = 0;
            NextPageID = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(contextFolderPath);
            Logger = provider.CreateLogger<PageManager>()!;
        }

        public async Task<IPage> CreateNewPrimaryPageAsync()
        {
            var pageCreationTask = new TaskCompletionSource<IPage>();

            Func<Task<IPage>> createPage = async () =>
            {
                try
                {
                    IPage page = await CreateAndRunPageAsync();
                    pageCreationTask.SetResult(page);
                }
                catch (Exception e)
                {
                    pageCreationTask.SetException(e);
                }
                finally
                {
                    ActivePageSemaphore.Release();
                    await ProcessNextPage();
                }

                return await pageCreationTask.Task;
            };

            PageRequestQueue.Enqueue(createPage);
            await ProcessNextPage();
            return await pageCreationTask.Task;
        }

        private async Task<IPage> CreateAndRunPageAsync()
        {
            IPage page = await Context.NewPageAsync();
            try
            {
                IncrementPageCount();

                int pageID = Interlocked.Increment(ref NextPageID);
                string pageFolderPath = LogManager.CreatePageFolder(ContextFolderPath, pageID);
                await ExecutePageAsync(page, pageID, pageFolderPath);
                DecrementPageCount();
            }
            catch (Exception e)
            {
                Logger.LogError($"Page execution failed: {e.Message}");
            }

            return page;
        }

        private async Task ExecutePageAsync(IPage page, int pageID, string pageFolderPath)
        {
            try
            {
                CustomLoggerProvider provider = new CustomLoggerProvider(pageFolderPath);
                CustomLogger PageLogger = provider.CreateLogger<PageManager>()!;

                Logger.LogInformation("Starting");
                await page.GotoAsync("https://www.google.com");
                await Task.Delay(10000);
                Logger.LogInformation("Google complete");
                await page.GotoAsync("https://example.com");
                await Task.Delay(10000);
                Logger.LogInformation("Example complete");
                await page.GotoAsync("https://www.bing.com");
                await Task.Delay(10000);
                Logger.LogInformation("Bing complete");
                await page.GotoAsync("https://www.yahoo.com");
                await Task.Delay(10000);
                Logger.LogInformation("Yahoo complete");
                await page.GotoAsync("https://www.wikipedia.org");
                await Task.Delay(10000);
                Logger.LogInformation("Wikipedia complete");
                await page.GotoAsync("https://www.reddit.com");
                await Task.Delay(10000);
                Logger.LogInformation("Reddit complete");
                await page.GotoAsync("https://www.microsoft.com");
                await Task.Delay(10000);
                Logger.LogInformation("Microsoft complete");
                await page.GotoAsync("https://www.apple.com");
                await Task.Delay(10000);
                Logger.LogInformation("Apple complete");
                await page.GotoAsync("https://www.amazon.com");
                await Task.Delay(10000);
                Logger.LogInformation("Amazon complete");
                await page.GotoAsync("https://www.netflix.com");
                await Task.Delay(10000);
                Logger.LogInformation("Netflix complete");
            }
            catch (Exception e)
            {
                Logger.LogError($"Page execution failed: {e.Message}");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private void IncrementPageCount()
        {
            Interlocked.Increment(ref ActivePageCount);
            Interlocked.Increment(ref PageCount);
            Logger.LogInformation($"Context '{ContextID}' : Adding Active Page -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}' | Total # of pages: '{PageCount}'");
        }

        private void DecrementPageCount()
        {
            Interlocked.Decrement(ref ActivePageCount);
            Interlocked.Decrement(ref PageCount);
            Logger.LogInformation($"Context '{ContextID}' : Removing Active Page -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}' | Total # of pages: '{PageCount}'");
        }

        private async Task ProcessNextPage()
        {
            if (await ActivePageSemaphore.WaitAsync(0) && PageRequestQueue.TryDequeue(out var nextTask))
            {
                Task.Run(nextTask); // DO NOT AWAIT
            }
            else if (PageRequestQueue.Count > 0)
            {
                Logger.LogInformation($"Context '{ContextID}' : Queuing Active Page -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}' | Total # of pages: '{PageCount}'");
            }
        }
    }
}
