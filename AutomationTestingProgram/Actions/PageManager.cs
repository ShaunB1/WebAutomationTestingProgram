using Microsoft.Identity.Client;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Actions
{
    public class PageManager
    {
        // If a list of 10+ pages exist (for an active page), close all the pages and active page, log an error!!

        private readonly IBrowserContext Context;
        private readonly int ContextID;
        private readonly SemaphoreSlim ActivePageSemaphore;
        private readonly ConcurrentQueue<Func<Task<IPage>>> PageRequestQueue;
        private readonly ConcurrentDictionary<IPage, List<IPage>> Pages; // Primary -> Secondarys
        private readonly ILogger<PageManager> Logger;

        private const int MaxActivePages = 3;

        private int ActivePageCount;
        private int PageCount;

        public PageManager(IBrowserContext context, int contextID, ILogger<PageManager> logger)
        {
            this.Context = context;
            this.ContextID = contextID;
            this.Logger = logger;
            this.ActivePageSemaphore = new SemaphoreSlim(MaxActivePages);
            this.PageRequestQueue = new ConcurrentQueue<Func<Task<IPage>>>();
            this.Pages = new ConcurrentDictionary<IPage, List<IPage>>();

            this.ActivePageCount = 0;
            this.PageCount = 0;
        }

        public async Task<IPage> CreateNewPrimaryPageAsync()
        {
            var pageCreationTask = new TaskCompletionSource<IPage>();

            Func<Task<IPage>> createPage = async () =>
            {
                await ActivePageSemaphore.WaitAsync();
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
                    ProcessNextPage();
                }

                return pageCreationTask.Task.Result;
            };

            PageRequestQueue.Enqueue(createPage);
            ProcessNextPage();
            return await pageCreationTask.Task;
        }

        private async Task<IPage> CreateAndRunPageAsync()
        {
            IPage page = await Context.NewPageAsync();
            try
            {
                IncrementPageCount();
                await ExecutePageAsync(page);
                DecrementPageCount();
            }
            catch (Exception e)
            {
                Logger.LogError($"Page execution failed: {e.Message}");
            }

            return page;
        }

        private async Task ExecutePageAsync(IPage page)
        {
            try
            {
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

        private void ProcessNextPage()
        {
            if (ActivePageSemaphore.CurrentCount > 0 && PageRequestQueue.TryDequeue(out var nextTask))
            {
                Task.Run(nextTask);
            } 
            else if (PageRequestQueue.Count > 0)
            {
                Logger.LogInformation($"Context '{ContextID}' : Queuing Active Page -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}' | Total # of pages: '{PageCount}'");
            }
        }



    }
}
