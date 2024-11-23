using AutomationTestingProgram.Services.Logging;
using Microsoft.Identity.Client;
using Microsoft.Playwright;
using System.Collections.Concurrent;
using AutomationTestingProgram.Models.Backend;

namespace AutomationTestingProgram.Backend
{
    public class PageManager
    {
        // If a list of 10+ pages exist (for an active page), close all the pages and active page, log an error!!

        private readonly Context Context;
        private readonly SemaphoreSlim ActivePageSemaphore;
        private readonly ConcurrentQueue<Func<Task<Page>>> PageRequestQueue;
        // private readonly ConcurrentDictionary<IPage, List<IPage>> Pages; // Primary -> Secondarys
        private readonly ILogger<PageManager> Logger;
        private int ActivePageCount;

        public PageManager(Context context)
        {
            Context = context;
            ActivePageSemaphore = new SemaphoreSlim(3);
            PageRequestQueue = new ConcurrentQueue<Func<Task<Page>>>();
            //Pages = new ConcurrentDictionary<IPage, List<IPage>>();
            ActivePageCount = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(context.FolderPath);
            Logger = provider.CreateLogger<PageManager>()!;
        }

        public async Task<Page> CreateNewPrimaryPageAsync()
        {
            var pageTask = new TaskCompletionSource<Page>();

            Func<Task<Page>> createPage = async () =>
            {
                Page page = new Page(Context);
                try
                {
                    IncrementPageCount(page);
                    await page.InitializeAsync();
                    await ExecutePageAsync(page);
                    pageTask.SetResult(page);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Context-Level Error encountered\n {e}");
                    pageTask.SetException(e);
                }
                finally
                {
                    DecrementPageCount(page);
                    ActivePageSemaphore.Release();
                    await ProcessNextPage();
                }

                return await pageTask.Task;
            };

            PageRequestQueue.Enqueue(createPage);
            await ProcessNextPage();
            return await pageTask.Task;
        }
        private async Task ExecutePageAsync(Page page)
        {
            try
            {
                var tasks = new[]
                {
                    page.RunAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                await page.CloseAllAsync();
            }
        }

        private void IncrementPageCount(Page page)
        {
            Interlocked.Increment(ref ActivePageCount);
            Logger.LogInformation($"Executing Active Page (ID: {page.ID}) -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}'");
        }

        private void DecrementPageCount(Page page)
        {
            Interlocked.Decrement(ref ActivePageCount);
            Logger.LogInformation($"Terminating Active Page (ID: {page.ID}) -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}'");
        }

        private async Task ProcessNextPage()
        {
            if (await ActivePageSemaphore.WaitAsync(0) && PageRequestQueue.TryDequeue(out var nextTask))
            {
                _ = Task.Run(nextTask); // DO NOT AWAIT
            }
            else if (PageRequestQueue.Count > 0)
            {
                Logger.LogInformation($"Queuing Active Page -- Count: '{ActivePageCount}' | Queue: '{PageRequestQueue.Count}'");
            }
        }
    }
}
