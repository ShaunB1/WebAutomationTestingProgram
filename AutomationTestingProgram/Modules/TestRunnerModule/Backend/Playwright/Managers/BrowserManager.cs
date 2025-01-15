/*using AutomationTestingProgram.Core.Helpers;
using AutomationTestingProgram.Core.Services.Requests;
using AutomationTestingProgram.Core.Settings.Playwright;
using AutomationTestingProgram.Modules.TestRunnerModule.Backend.Playwright.Objects;
using AutomationTestingProgram.Modules.TestRunnerModule.Backend.Requests.TestController;
using AutomationTestingProgram.Services.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Backend.Playwright.Managers
{
    /// <summary>
    /// Represents the Manager that manages <see cref="Browser"/> objects.
    /// </summary>
    public class BrowserManager
    {
        private static readonly BrowserSettings _settings;

        /// <summary>
        /// The PlaywrightObject Parent Instance.
        /// </summary>
        private readonly PlaywrightObject _playwright;

        /// <summary>
        /// Limits the number of browsers that can run concurrently at a time
        /// </summary>
        private readonly SemaphoreSlim _maxBrowsers;

        /// <summary>
        /// Browser Queue.
        /// </summary>
        private readonly ConcurrentQueue<(string Type, string Version)> _queuedBrowsers;

        /// <summary>
        /// Dictionary that tracks a list of requests waiting for a specific browser. Used in tandem with BrowserQueue
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), List<IClientRequest>> _queuedRequests;

        /// <summary>
        /// Dictionary that tracks all currently active browsers, mapped by browser type/version.
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), Browser> _activeBrowsers;

        /// <summary>
        /// Ensures that only one request for a particular browser can be processed by BrowserManager at a time
        /// </summary>
        private readonly LockManager<(string Type, string Version)> _lockManager;

        /// <summary>
        /// The Logger object associated with this class
        /// </summary>
        private readonly ILogger<BrowserManager> _logger;

        static BrowserManager()
        {
            _settings = AppConfiguration.GetSection<BrowserSettings>("Browser");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserManager"/> class.
        /// This class manages <see cref="Browser"/> objects.
        /// </summary>
        /// <param name="playwright"></param>
        public BrowserManager(PlaywrightObject playwright)
        {
            _playwright = playwright;
            _maxBrowsers = new SemaphoreSlim(_settings.Limit);
            _queuedBrowsers = new ConcurrentQueue<(string Type, string Version)>();
            _queuedRequests = new ConcurrentDictionary<(string Type, string Version), List<IClientRequest>>();
            _activeBrowsers = new ConcurrentDictionary<(string Type, string Version), Browser>();
            _lockManager = new LockManager<(string Type, string Version)>();

            CustomLoggerProvider provider = new CustomLoggerProvider(LogManager.GetRunFolderPath());
            _logger = provider.CreateLogger<BrowserManager>()!;
        }

        /// <summary>
        /// Processes a request by either executing it on an existing browser or queuing it if no suitable browser is available.
        /// </summary>
        /// <param name="request">The request to process. Must be a ProcessRequest.</param>
        /// <returns>The completed request</returns>
        public async Task ProcessRequestAsync(ProcessRequest request)
        {
            await request.IsCancellationRequested();

            request.SetStatus(State.Received, "Browser Manager Received Request");

            // Retrieving the browser lock for the request, ensuring no concurrency issues
            await _lockManager.AquireLockAsync((request.BrowserType, request.BrowserVersion));

            request.SetStatus(State.Processing, "Browser Manager Processing Request");

            bool processNextBrowser = false; // Needed to prevent deadlock

            try
            {
                // Check if the browser is currently active
                if (_activeBrowsers.TryGetValue((request.BrowserType, request.BrowserVersion), out Browser? browser))
                {
                    // Should return once successfully started. Completion is kept within request
                    await ExecuteRequestAsync(browser, request);
                }
                // Else, queue request
                else
                {
                    processNextBrowser = QueueNewRequest(processRequest);
                }
            }
            catch (Exception e)
            {
                request.SetStatus(State.Failure, "Browser Manager Processing: Failure", e);
            }
            finally
            {
                _lockManager.ReleaseLock((request.BrowserType, request.BrowserVersion));
                if (processNextBrowser)
                    await ProcessNextBrowser();
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
    }
}
}
*/