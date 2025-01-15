/*using AutomationTestingProgram.Core.Services.Requests;
using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Modules.TestRunnerModule.Backend.Playwright.Managers;
using AutomationTestingProgram.Modules.TestRunnerModule.Backend.Playwright.Objects;
using AutomationTestingProgram.Modules.TestRunnerModule.Backend.Requests.TestController;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Playwright;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Represents the Manager that manages <see cref="Browser"/> objects.
    /// </summary>
    public class BrowserManagerV1
    {   
        /// <summary>
        /// The PlaywrightObject Parent Instance.
        /// </summary>
        private readonly PlaywrightObject Playwright;

        /// <summary>
        /// Limits the number of browsers that can run concurrently at a time
        /// </summary>
        private readonly SemaphoreSlim BrowserSemaphore;

        /// <summary>
        /// Queue for browser creation tasks. Each task is responsible for creating a new browser instance.
        /// </summary>
        private readonly ConcurrentQueue<Func<Task<Browser>>> BrowserQueue;

        /// <summary>
        /// Dictionary that tracks a list of requests waiting for a specific browser. Used in tandem with BrowserQueue
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), List<IClientRequest>> QueuedRequests;

        /// <summary>
        /// Dictionary that tracks all currently active browsers, mapped by browser type/version.
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), Browser> ActiveBrowsers;

        /// <summary>
        /// Dictionary of semaphores, one for each browser type/version combination.
        /// Ensures that only one request for a particular browser can be processed by BrowserManager at a time
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), SemaphoreSlim> BrowserLocks;

        /// <summary>
        /// The Logger object associated with this class
        /// </summary>
        private readonly ILogger<BrowserManager> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserManager"/> class.
        /// This class manages <see cref="Browser"/> objects.
        /// </summary>
        /// <param name="playwright"></param>
        public BrowserManager(PlaywrightObject playwright)
        {
            Playwright = playwright;
            BrowserSemaphore = new SemaphoreSlim(3); // Current limit of 3 Browser objects active at a time
            BrowserQueue = new ConcurrentQueue<Func<Task<Browser>>>();
            QueuedRequests = new ConcurrentDictionary<(string Type, string Version), List<IClientRequest>>();
            ActiveBrowsers = new ConcurrentDictionary<(string Type, string Version), Browser>();
            BrowserLocks = new ConcurrentDictionary<(string Type, string Version), SemaphoreSlim>();

            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
            Logger = provider.CreateLogger<BrowserManager>()!;
        }

        /// <summary>
        /// Processes a request by either executing it on an existing browser or queuing it if no suitable browser is available.
        /// A TaskCompletionSource is used to handle the asynchronous completion of the request.
        /// </summary>
        /// <param name="request">The request to process</param>
        /// <returns>The completed request</returns>
        public async Task ProcessRequestAsync(IClientRequest request)
        {
            *//*
             * Only for ProcessRequests currently.
             * Will re-factor if new requests added here.
             *//*

            if (!(request is ProcessRequest processRequest)) return;

            await processRequest.IsCancellationRequested();
            
            processRequest.SetStatus(State.Processing, "Browser Manager Processing Request");

            // Retrieving the browser lock for the request, ensuring no concurrency issues
            var browserLock = BrowserLocks.GetOrAdd((processRequest.BrowserType, processRequest.BrowserVersion), _ => new SemaphoreSlim(1));
            await browserLock.WaitAsync();

            bool processNextBrowser = false; // Needed to prevent deadlock

            try
            {
                // Check if the browser is currently active
                if (ActiveBrowsers.TryGetValue((processRequest.BrowserType, processRequest.BrowserVersion), out Browser? browser))
                {
                    // Should return once successfully started. Completion is kept within request
                    await ExecuteRequestAsync(browser, processRequest);
                }
                // Else, queue request
                else
                {
                    processNextBrowser = QueueNewRequest(processRequest);
                }
            }
            catch (Exception e)
            {
                request.SetStatus(State.Failure, "Browser Manager Failure", e);
            }
            finally
            {
                browserLock.Release();
                if (processNextBrowser)
                    await ProcessNextBrowser();
            }
        }

        /// <summary>
        /// Queues a new request to be processed by a browser. If a browser of the requested type is already in the queue,
        /// the request will be added to it's list of request. Else, a new browser creation task will be queued.
        /// </summary>
        /// <param name="request">The request to queue</param>
        /// <returns>A boolean value determining whether ProcessNextBrowser() should be called</returns>
        private bool QueueNewRequest(ProcessRequest request)
        {
            request.SetStatus(State.Queued, "Browser Manager Queued Request");

            // If Browser already in queue, add request to list
            if (!QueuedRequests.TryAdd((request.BrowserType, request.BrowserVersion), new List<IClientRequest> { request }))
            {
                QueuedRequests.TryGetValue((request.BrowserType, request.BrowserVersion), out List<IClientRequest>? requests);
                requests!.Add(request); // Note: Because of the lock, no two requests will add to the list at the same time                
                return false;
            }
            // Else, add to queue for first time. Return true as ProcessNextBrowser must be called
            else
            {
                // First time adding to dictionary -> add to queue
                BrowserQueue.Enqueue(CreateNewBrowserTask(request.BrowserType, request.BrowserVersion));
                return true;
            }
        }

        /// <summary>
        /// Creates a new task that, when executed, will create and initialize a new browser instance.
        /// The task will also handle any queued requests for that browser, executing them once the browser is ready.
        /// </summary>
        /// <param name="Type">The type of the browser</param>
        /// <param name="Version">The version of the browser</param>
        /// <returns>A task that will initialize a browser and process related queued requests when run</returns>
        private Func<Task<Browser>> CreateNewBrowserTask(string Type, string Version)
        { 
            Func<Task<Browser>> createBrowser = async () =>
            {
                // Locking browser when moving from queue to active
                var browserLock = BrowserLocks.GetOrAdd((Type, Version), _ => new SemaphoreSlim(1));
                await browserLock.WaitAsync();

                Browser browser = new Browser(Playwright, Type, Version);
                IncrementBrowserCount(browser);
                try
                {   
                    // Initialization possible to fail if takes too long.. -> retry mechanism??
                    await browser.InitializeAsync();
                    if (QueuedRequests.TryRemove((browser.Type, browser.Version), out List<IClientRequest>? requests))
                    {
                        foreach (IClientRequest request in requests)
                        {
                            // Should return once successfully started. Completion is kept within request
                            await ExecuteRequestAsync(browser, request);
                        }
                    }                    
                    return browser;
                }
                finally
                {
                    browserLock.Release();
                }
            };

            return createBrowser;
        }


        /// <summary>
        /// Terminates a browser.
        /// Called by the Context manager of a particular browser
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        public async Task TerminateBrowserAsync(Browser browser)
        {
            if (BrowserLocks.TryGetValue((browser!.Type, browser!.Version), out SemaphoreSlim? browserLock))
            {

                bool processNextBrowser = false;

                try
                {
                    await browserLock.WaitAsync();

                    // Successfully closed
                    if (await browser.CloseAsync())
                    {   
                        DecrementBrowserCount(browser);
                        BrowserSemaphore.Release();
                        processNextBrowser = true;
                    }
                    // Else a new request was just received
                }
                catch (Exception e)
                {
                    Logger.LogError($"Run-Level Error encountered\n {e}");
                    throw;
                }
                finally
                {
                    browserLock.Release();
                    // Flushes to make sure all logs are outputted when a browser closes
                    if (Logger is CustomLogger<BrowserManager> customLogger)
                    {
                        customLogger.Flush();
                    }

                    if (processNextBrowser)
                    {
                        await ProcessNextBrowser();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a request to the browser for processing. This will initiate the request but does not wait for it to complete.
        /// Browsers can handle multiple requests simultaneously as long as they are not closed.
        /// </summary>
        /// <param name="browser">The browser instance that will handle the request</param>
        /// <param name="request">The request to be processed by the browser</param>
        /// <returns></returns>
        private async Task ExecuteRequestAsync(Browser browser, IClientRequest request)
        {
            await browser.ProcessRequest(request);
        }

        /// <summary>
        /// Increment the total # of running browsers
        /// </summary>
        /// <param name="browser"></param>
        private void IncrementBrowserCount(Browser browser)
        {
            ActiveBrowsers.TryAdd((browser.Type, browser.Version), browser);
            Logger.LogInformation($"Executing Browser (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}) -- Browsers Running: '{ActiveBrowsers.Count}' | Queued: '{BrowserQueue.Count}'");
        }

        /// <summary>
        /// Decrement the total # of running browsers.
        /// </summary>
        /// <param name="browser"></param>
        public void DecrementBrowserCount(Browser browser)
        { 
            ActiveBrowsers.TryRemove((browser.Type, browser.Version), out var value);
            Logger.LogInformation($"Terminating Browser (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}) -- Browsers Running: '{ActiveBrowsers.Count}' | Queued: '{BrowserQueue.Count}'");
        }

        /// <summary>
        /// Called when a new request in a new browser is sent, ensuring browser processing if there is available capacity.
        /// Also called when a browser terminates execution, as it frees up a new slot for additional requests.
        /// </summary>
        /// <returns></returns>
        public async Task ProcessNextBrowser()
        {   
            // Tries to process immediatelly using WaitAsync(0). If no spot, returns.
            if (await BrowserSemaphore.WaitAsync(0))
            {
                // Start processing the task. TaskCompletionSource above ensures completion
                if (BrowserQueue.TryDequeue(out var nextTask))
                {
                    await Task.Run(nextTask);
                }
                else
                {
                    BrowserSemaphore.Release();
                }

            }
            // Only logs if the queue not processing request (queued)
            else if (BrowserQueue.Count > 0)
            {
                Logger.LogInformation($"Queuing Browser -- Browsers Running: '{ActiveBrowsers.Count}' | Queued: '{BrowserQueue.Count}'");
            }

            *//*
             * INFO:
             * 
             * ProcessNextBrowser is only called either when a new browser is queued or when a browser
             * terminates. 
             * 
             * In the first case:
             *  - Needed to start the whole process (if there are no active browsers to terminate)
             *  - If no slots, it means it stays in the queue, waiting for termination
             * In the second case:
             *  - When a browser terminates, we need to start the next one from the queue
             *  
             * If multiple terminate at the same time, simply multiple will start at the same time.
             * If terminate and add at the same time, one queue, one start.
             *  -> No concurrency issues with Queuing Browser message
             *//*
        }
    }
}
*/