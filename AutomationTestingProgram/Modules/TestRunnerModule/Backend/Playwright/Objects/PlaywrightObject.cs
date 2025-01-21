/*using AutomationTestingProgram.Core;
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
        private readonly IPlaywright Instance;

        /// <summary>
        /// Settings used for Browser Creation/Management
        /// </summary>
        private readonly BrowserSettings Settings;

        /// <summary>
        /// Dictionary that tracks all currently active browsers, mapped by browser type/version.
        /// Each browser also keeps track of total # of active requests.
        /// </summary>
        private readonly ConcurrentDictionary<(string Type, string Version), (Browser Browser, int Amount)> ActiveBrowsers;

        /// <summary>
        /// Ensures that each browser can only process one event (Request added/terminated) at a time.
        /// Includes a limit to how many browsers can be active at a time.
        /// </summary>
        private readonly LockManager<(string Type, string Version)> LockManager;

        private ICustomLogger Logger;

        /// <summary>
        /// Keeps track of the next unique identifier for browser instances created by this object
        /// </summary>
        private int NextBrowserID;

        *//* INFO:
         * - Requests have unique IDs
         * - Playwright, Browser, Contexts, Pages have unique ID's within their parent
         *   This means that its possible for two Pages to have ID 1, but originate form different parents.
         *   Therefore, unique ID of objects per run are:
         *      Browser -> Browser ID within a run
         *      Context -> Parent (Browser ID), Context ID within parent
         *      Page -> Grandparent (Browser ID), Parent (Context ID), Page ID within parent
         * - Note: Requests and Context folders will link. Therefore, unique ID is more important request side.
         *//*

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaywrightObject"/> class.
        /// to manage <see cref="Browser"/> instances.
        /// </summary>
        public PlaywrightObject(ICustomLoggerProvider provider, IOptions<BrowserSettings> options)
        {
            Instance = Playwright.CreateAsync().GetAwaiter().GetResult();
            Settings = options.Value;
            ActiveBrowsers = new ConcurrentDictionary<(string Type, string Version), (Browser Browser, int Amount)>();
            LockManager = new LockManager<(string Type, string Version)>(Settings.Limit);
            Logger = provider.CreateLogger<PlaywrightObject>();
            NextBrowserID = 0;
        }

        /// <summary>
        /// Processes a request by either executing it on an existing browser or queuing it if no suitable browser is available.
        /// </summary>
        /// <param name="request">The request to process.</param>
        public async Task ProcessRequestAsync(ProcessRequest request)
        {
            *//*
             * Playwright operations are only for ProcessRequests for now.
             * Refactor can later occur.
             *//*

            request.SetStatus(State.Processing, "Playwright processing request.");
            request.IsCancellationRequested();

            request.LogInfo($"Waiting for lock on Browser Type: {request.BrowserType}, Version: {request.BrowserVersion}.");
            await LockManager.AquireLockAsync((request.BrowserType, request.BrowserVersion), request.CancelToken);
            request.LogInfo($"Lock aquired");

            Browser browser;

            try
            {
                if (ActiveBrowsers.TryGetValue((request.BrowserType, request.BrowserVersion), out var entry))
                {
                    // If Browser already active, increase linked Amount
                    entry.Amount++;
                    browser = entry.Browser;
                }
                else
                {
                    // Create Browser, set linked Amount to 1
                    browser = new Browser(this, request.BrowserType, request.BrowserVersion);
                    await browser.InitializeAsync();
                    ActiveBrowsers.TryAdd((request.BrowserType, request.BrowserVersion), (browser, 1));
                }
            }
            finally
            {
                LockManager.ReleaseLock((request.BrowserType, request.BrowserVersion));
            }

            try
            {
                await browser.ProcessRequest(request);
            }
            finally
            {
                await LockManager.AquireLockAsync((request.BrowserType, request.BrowserVersion));
                if (ActiveBrowsers.TryGetValue((request.BrowserType, request.BrowserVersion), out var entry))
                {
                    entry.Amount--;

                    if (entry.Amount == 0)
                    {

                    }
                }   
                LockManager.ReleaseLock((request.BrowserType, request.BrowserVersion));
            }
            


        }
    }
}
*/