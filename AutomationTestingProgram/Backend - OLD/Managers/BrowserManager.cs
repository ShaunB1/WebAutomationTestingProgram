/*using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend.Managers
{   
    /// <summary>
    /// Represents the Manager that manages <see cref="Browser"/> objects.
    /// </summary>
    public class BrowserManager
    {   
        /// <summary>
        /// The PlaywrightObject Parent Instance.
        /// </summary>
        private readonly PlaywrightObject Playwright;

        /// <summary>
        /// Allows up to '3' browsers to run concurrently at a time, per PlaywrightObject Instance
        /// </summary>
        private readonly SemaphoreSlim BrowserSemaphore;

        /// <summary>
        /// Queue for browser creation tasks. Each task is responsible for creating a new browser instance.
        /// </summary>
        private readonly ConcurrentQueue<Func<Task<Browser>>> BrowserQueue;

        /// <summary>
        /// Dictionary that tracks a list of requests waiting for a specific browser. Used in tandem with BrowserQueue
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, int Version), List<Request>> QueuedBrowsers;

        /// <summary>
        /// Dictionary that tracks all currently active browsers, mapped by browser type/version.
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, int Version), Browser> ActiveBrowsers;

        /// <summary>
        /// Dictionar of semaphores, one for each browser type/version combination.
        /// Ensures that only one request for a particular browser can be processed by BrowserManager at a time
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, int Version), SemaphoreSlim> BrowserLocks;
        *//*
         * The BrowserLocks dictionary is used to prevent concurrency issues when transitioning requests
         * from the queued to active states, while also allowing new requests to be received simultaneously.
         * It ensures that requests for the same browser (type/version) are processed sequentially,
         * while allowing requests for different browsers to run concurrently -> within BrowserManager
         * 
         * Potential issue: Entires in the dictionary cannot be removed once created. While this could lead to unecessary
         *                  memory usage, the amount of entires is expected to be small, so this is an insignificant issue.
         *//*

        /// <summary>
        /// The Logger object associated with this class
        /// </summary>
        private readonly ILogger<BrowserManager> Logger;

        /// <summary>
        /// Contains the number of queued requests within the BrowserManager class.
        /// </summary>
        private int QueuedRequestCount;

        /// <summary>
        /// Contains the number of 'sent' requests within the BrowserManager class.
        /// Note: A sent request is not necessarily active -> May be queued in ContextManager
        /// </summary>
        private int SentRequestCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserManager"/> class.
        /// This class manages <see cref="Browser"/> objects.
        /// </summary>
        /// <param name="playwright"></param>
        public BrowserManager(PlaywrightObject playwright)
        {
            Playwright = playwright;
            BrowserSemaphore = new SemaphoreSlim(3);
            BrowserQueue = new ConcurrentQueue<Func<Task<Browser>>>();
            QueuedBrowsers = new ConcurrentDictionary<(string Type, int Version), List<Request>>();
            ActiveBrowsers = new ConcurrentDictionary<(string Type, int Version), Browser>();
            BrowserLocks = new ConcurrentDictionary<(string Type, int Version), SemaphoreSlim>();

            QueuedRequestCount = 0;
            SentRequestCount = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
            Logger = provider.CreateLogger<BrowserManager>()!;
        }

        /// <summary>
        /// Processes a request by either executing it on an existing browser or queuing it if no suitable browser is available.
        /// A TaskCompletionSource is used to handle the asynchronous completion of the request.
        /// </summary>
        /// <param name="request">The request to process</param>
        /// <returns>The completed request</returns>
        public async Task<Request> ProcessRequestAsync(Request request)
        {
            // Creating the task completion source, and linking it to the request
            var requestTask = new TaskCompletionSource<Request>();
            request.ResponseSource = requestTask;
            request.SetStatus(RequestState.Processing, "Browser Manager Processing Request");

            // Retrieving the browser lock for the request, ensuring no concurrency issues
            var browserLock = BrowserLocks.GetOrAdd((request.BrowserType, request.BrowserVersion), _ => new SemaphoreSlim(1));
            await browserLock.WaitAsync(); // Ensures that browsers can only process one request at a time

            // Used to determine whether ProcessNextBrowser() should be called -> Only when queuing a new browser for the first time
            bool processNextBrowser = false;

            try
            {
                // Check if the browser is currently active
                if (ActiveBrowsers.TryGetValue((request.BrowserType, request.BrowserVersion), out Browser? browser))
                {
                    // Should return once successfully started. Completion is kept within request
                    await ExecuteRequestAsync(browser, request);
                }
                // Else, queue request
                else
                {   
                    processNextBrowser = QueueNewRequest(request);
                }
            }
            catch (Exception e)
            {
                request.SetStatus(RequestState.ProcessingFailure, "Browser Manager Processing Request: Failure", e);
            }
            finally
            {
                browserLock.Release();
                // If queued for first time, call ProcessNextBrowser()
                if (processNextBrowser)
                    await ProcessNextBrowser();
            }            

            return await request.ResponseSource.Task;
        }

        /// <summary>
        /// Queues a new request to be processed by a browser. If a browser of the requested type is already in the queue,
        /// the request will be added to it's list of request. Else, a new browser creation task will be queued.
        /// </summary>
        /// <param name="request">The request to queue</param>
        /// <returns>A boolean value determining whether ProcessNextBrowser() should be called</returns>
        private bool QueueNewRequest(Request request)
        {
            request.SetStatus(RequestState.Queued, "Browser Manager Queued Request");
            Interlocked.Increment(ref QueuedRequestCount);

            // If Browser already in queue, add request to list
            if (!QueuedBrowsers.TryAdd((request.BrowserType, request.BrowserVersion), new List<Request> { request }))
            {
                QueuedBrowsers.TryGetValue((request.BrowserType, request.BrowserVersion), out List<Request>? requests);
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
        private Func<Task<Browser>> CreateNewBrowserTask(string Type, int Version)
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
                    // Initialization possible to fail -> not coding issue. Should I add retry mechanism?? ** ** ** ** ** ** ** ** ** ** ** ** ** **
                    await browser.InitializeAsync();
                    if (QueuedBrowsers.TryRemove((browser.Type, browser.Version), out List<Request>? requests))
                    {
                        foreach (Request request in requests)
                        {
                            // Should return once successfully started. Completion is kept within request
                            Interlocked.Decrement(ref QueuedRequestCount);
                            await ExecuteRequestAsync(browser, request);
                        }
                    }                    
                    return browser;
                }
                catch (Exception e)
                {   
                    *//* Only thing in try block that could fail is InitializeAsync
                     * This would occur if too heavy a load, and timeout not enough.
                     * Must test to see if an issue, and if a retry mechasim is needed
                     * 
                     * However, if InitializeAsync() fails, we correctly log, remove it from active and release the semaphore.
                     * Also, as we never removed the requests from the queue, no requests will be stranded.
                     * Issue -> If fails, only two active browsers will exist, and no 3rd ever. To fix this,
                     * Process Next Browser should be called.
                     * 
                     * !!! Before fixing/working on this, must test load, and see if browsers fail..
                     * 
                     * 
                     *//*

                    Logger.LogError($"Run-Level Error encountered\n {e}");
                    DecrementBrowserCount(browser);
                    BrowserSemaphore.Release();
                    *//*
                     * await ProcessNextBrowser();
                     * return null;
                     *//*
                    throw;
                }
                finally
                {
                    browserLock.Release();
                }
            };

            return createBrowser;
        }

        *//* Low likeyhood race condition issue:
         * - ContextManager detects that it wants to close the browser. 
         * - A new request just comes in
         * - TerminateBrowserAsync has to wait for lock, taken by processnewrequest
         * - Request sent to browser manager -> now active.
         * - Lock is released. TerminateBrowserAsync gets the lock.
         * - Request terminates immediatelly. Tries to close.
         * - TerminateBrowserAsync detects that a request is active, just before it terminates and decreases activerequestcount
         * - TerminateBrowserAsync doesnt close browser. Releases lock
         * - Request tries to send a new request to terminate, but fails because the lock wasn't yet released
         * 
         * To fix this, ContextManagers must await the lock
         * Issue:
         * - Possible multiple calls to TerminateBrowserAsync, while the brow is already closed/terminated. This will result in a failure. 
         * 
         * 
         * 
         *//*


        /// <summary>
        /// Terminates a browser.
        /// Called by the Context manager of a particular browser
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task TerminateBrowserAsync(Browser browser)
        {
            if (BrowserLocks.TryGetValue((browser!.Type, browser!.Version), out SemaphoreSlim? browserLock))
            {

                bool success = false;

                try
                {
                    await browserLock.WaitAsync();
                    if (browser.IsTerminated)
                        return;

                    // Successfully closed
                    if (await browser.CloseAsync())
                    {   
                        DecrementBrowserCount(browser);
                        BrowserSemaphore.Release();
                        success = true;
                        browser.IsTerminated = true;
                    }
                    // Else a new context was just added (concurrency)
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

                    if (success)
                    {
                        await ProcessNextBrowser();
                    }
                }
            }
        }

        /// <summary>
        /// Increment the total # of processing requests RUN LEVEL
        /// Called whenever a new request is received by ContextManager (queued or active)
        /// </summary>
        /// <param name="browser"></param>
        public void IncrementRequestCount(Browser browser, Request request)
        {
            Interlocked.Increment(ref SentRequestCount);
            Logger.LogInformation($"New Request (ID: {request.ID}) sent to Browser (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}) " +
                $"-- Total Requests Sent: '{SentRequestCount}' | Queued: '{QueuedRequestCount}'");
        }

        /// <summary>
        /// Decrement the total # of processing requests RUN LEVEL
        /// Called whenever a request is terminated in ContextManager
        /// Note: BrowserManager does not know when a request terminates. MUST BE CALLED FROM CONTEXT MANAGER
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="successful"></param>
        public void DecrementRequestCount(Browser browser, Request request)
        { 
            Interlocked.Decrement(ref SentRequestCount);
            Logger.LogInformation($"Terminating Request (ID: {request.ID}, State: {request.State}) in Browser (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}) " +
                $"-- Total Requests Sent: '{SentRequestCount}' | Queued: '{QueuedRequestCount}'");
        }

        /// <summary>
        /// Sends a request to the browser for processing. This will initiate the request but does not wait for it to complete.
        /// Browsers can handle multiple requests simultaneously as long as they are not closed.
        /// </summary>
        /// <param name="browser">The browser instance that will handle the request</param>
        /// <param name="request">The request to be processed by the browser</param>
        /// <returns></returns>
        private async Task ExecuteRequestAsync(Browser browser, Request request)
        {
            try
            {
                request.SetStatus(RequestState.Processing, "Browser Manager Sending Request -> Browser");
                
                // Send the request to the browser for processing. This initiates the process but does not wait for completion
                await browser.ProcessRequest(request);
            }
            catch (Exception e) // Should not occur
            {   // Processing failed
                request.SetStatus(RequestState.ProcessingFailure, "Browser Manager Sending Request -> Browser: Failure", e);
            }
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
        }
    }
}
*/