using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Represents an instance of Playwright.
    /// </summary>
    public class PlaywrightObject
    {
        /// <summary>
        /// The IPlaywright Instance linked with this object
        /// </summary>
        public readonly IPlaywright Instance;



        /// <summary>
        /// Settings used for Browser Management
        /// </summary>
        private readonly PlaywrightSettings _settings;

        /// <summary>
        /// Factory used to create Browser Instances
        /// </summary>
        private readonly IBrowserFactory _browserFactory;

        /// <summary>
        /// Dictionary that tracks all currently active browsers, mapped by browser type/version.
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), Browser> _activeBrowsers;

        /// <summary>
        /// Ensures that each browser can only process one event (Request added/terminated) at a time.
        /// Includes a limit to how many browsers can be active at a time.
        /// </summary>
        private readonly LockManager<(string Type, string Version)> _lockManager;

        /// <summary>
        /// Keeps track of the next unique identifier for browser instances created by this object
        /// </summary>
        private int _nextBrowserID;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ICustomLogger _logger;

        /* INFO:
         * - Requests have unique IDs
         * - Playwright, Browser, Contexts, Pages have unique ID's within their parent, within a run.
         *   This means that its possible for two Pages to have ID 1, but originate form different parents.
         *   Therefore, unique ID of objects per run are:
         *      Browser -> Browser ID within a run
         *      Context -> Parent (Browser ID), Context ID within parent
         *      Page -> Grandparent (Browser ID), Parent (Context ID), Page ID within parent
         * - Note: Requests and Context folders will link. Therefore, unique ID is more important request side.
         */

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaywrightObject"/> class.
        /// to manage <see cref="Browser"/> instances.
        /// </summary>
        public PlaywrightObject(ICustomLoggerProvider provider, IOptions<PlaywrightSettings> options, IBrowserFactory browserFactory)
        {
            Instance = Playwright.CreateAsync().GetAwaiter().GetResult();

            _settings = options.Value;
            _browserFactory = browserFactory;
            _activeBrowsers = new ConcurrentDictionary<(string Type, string Version), Browser>();
            _lockManager = new LockManager<(string Type, string Version)>(_settings.BrowserLimit);
            _logger = provider.CreateLogger<PlaywrightObject>();
            _nextBrowserID = 0;
        }

        /// <summary>
        /// Processes a request by either executing it on an existing browser or queuing it if no suitable browser is available.
        /// </summary>
        /// <param name="request">The request to process.</param>
        public async Task ProcessRequestAsync(ProcessRequest request)
        {
            /*
             * Playwright operations are only for ProcessRequests for now.
             * Refactor can later occur.
             * 
             * INFO:
             * - Aquires lock
             * - Retrieves or creates new Browser
             * - Adjustes request count within lock
             * - Exists lock, processes request in browser
             * - Regardless of result, we re-enter lock, decrement count
             * - Browser closed if count is 0
             */

            request.LogInfo("Playwright received request.");

            request.SetStatus(State.Queued, $"Waiting for lock on Browser Type: {request.BrowserType}, Version: {request.BrowserVersion}.");
            await _lockManager.AquireLockAsync((request.BrowserType, request.BrowserVersion), request.CancelToken);
            request.SetStatus(State.Processing, $"Lock aquired");

            Browser browser;
            try
            {
                if (_activeBrowsers.TryGetValue((request.BrowserType, request.BrowserVersion), out browser))
                {
                    // If Browser already active, increase linked Amount
                    browser.IncrementRequestCount(request);
                    request.LogInfo($"Active Browser Found...");
                }
                else
                {
                    // Create Browser, set linked Amount to 1
                    browser = await _browserFactory.CreateBrowser(this, request.BrowserType, request.BrowserVersion);
                    _activeBrowsers.TryAdd((request.BrowserType, request.BrowserVersion), browser);
                    _logger.LogInformation($"New Browser initialized (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}).");
                    request.LogInfo($"New Browser Created...");
                }
            }
            catch (LaunchException e)
            {
                _logger.LogError($"{e.Message}\nError:{e.InnerException}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception occured: {e}");
                throw;
            }
            finally
            {
                _lockManager.ReleaseLock((request.BrowserType, request.BrowserVersion));
            }

            try
            {
                await browser.ProcessRequest(request);
            }
            finally
            {
                // Browser is guaranteed to be active, so wait in LockManager is short
                await _lockManager.AquireLockAsync((request.BrowserType, request.BrowserVersion));
                browser.DecrementRequestCount(request);

                if (browser.GetRequestCount() == 0)
                {
                    await browser.CloseAsync();
                    _activeBrowsers.TryRemove((request.BrowserType, request.BrowserVersion), out _);
                    _logger.LogInformation($"Browser closed (ID: {browser.ID}, Type: {browser.Type}, Version: {browser.Version}).");
                }
                _lockManager.ReleaseLock((request.BrowserType, request.BrowserVersion));
            }
        }

        /// <summary>
        /// Gets the id -> used by browser objects created from this PlaywrightObject
        /// </summary>
        /// <returns></returns>
        public int GetNextBrowserID()
        {
            return Interlocked.Increment(ref _nextBrowserID);
        }
    }
}