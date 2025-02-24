using Microsoft.Graph.Models;
using Microsoft.Playwright;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Represents a browser object that manages browser instances, contexts, and related tasks.
    /// </summary>
    public class Browser
    {
        /// <summary>
        /// The current instance of the browser, which is created and managed by Playwright.
        /// </summary>
        public IBrowser? Instance { get; private set; }

        /// <summary>
        /// A unique identifier for this Browser object, specific to the parent Playwright instance.
        /// Note: This ID is not globally unique across all browsers, just those with the same parent.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// The filepath where the browser's folder is located, including logs and other related data
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// The Type of Browser
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The Version of Browser
        /// </summary>
        public string Version { get; }


        /// <summary>
        /// The parent Playwright instance that this Browser object belongs to.
        /// </summary>
        private PlaywrightObject _parent { get; }

        /// <summary>
        /// Settings used for Browser Creation/Context Management.
        /// </summary>
        private BrowserSettings _settings { get; }

        /// <summary>
        /// Factory used to create Context Instances
        /// </summary>
        private IContextFactory _contextFactory { get; }

        /// <summary>
        /// SemaphoreSlim limiting total # of active contexts
        /// </summary>
        private readonly SemaphoreSlim _contextLimit;

        /// <summary>
        /// Keeps track of the next unique idetifier for context instances created by this object
        /// </summary>
        private int _nextContextID;

        /// <summary>
        /// Keeps track of the # of active requests in this browser object.
        /// </summary>
        private int _requestCount;

        /// <summary>
        /// The Logger object associated with this class
        /// </summary>
        private readonly ICustomLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Browser"/> class.
        /// This constructor does not launch or initialize the browser, it only sets up the basic properties.
        /// Please call InitializeAsync() to finish set-up.
        /// </summary>
        /// <param name="playwright">The parent Playwright object that manages the browser.</param>
        /// <param name="type">The type of the browser (e.g., "chrome", "firefox").</param>
        /// <param name="version">The version of the browser (e.g., 91, 92).</param>
        public Browser(PlaywrightObject playwright, string type, string version, IOptions<BrowserSettings> options, ICustomLoggerProvider provider, IContextFactory contextFactory)
        {
            ID = playwright.GetNextBrowserID();
            Type = type;
            Version = version;
            FolderPath = LogManager.CreateBrowserFolder(ID, Type, Version);

            _parent = playwright;
            _settings = options.Value;
            _contextFactory = contextFactory;            
            _contextLimit = new SemaphoreSlim(_settings.ContextLimit);
            _logger = provider.CreateLogger<Browser>(FolderPath);
            _nextContextID = 0;
            _requestCount = 0;
        }

        /// <summary>
        /// Initializes the browser by creating a new browser instance.
        /// This method must be called after the browser object has been created.
        /// </summary>
        /// <exception cref="LaunchException">Thrown if the browser cannot be initialized.</exception>
        public async Task InitializeAsync()
        {
            try
            {
                Instance = await CreateBrowserInstance(_parent.Instance, Type, Version);
                _logger.LogInformation($"Successfully initialized Browser (ID: {ID}, Type: {Type}, Version: {Version})");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to initialize Browser (ID: {ID}, Type: {Type}, Version: {Version}) due to {e}");
                throw new LaunchException($"Failed to initialize Browser (ID: {ID}, Type: {Type}, Version: {Version}).", e);
            }
        }

        /// <summary>
        /// Sends a request to the Context refor processing.
        /// </summary>
        /// <param name="request">The request to process within the browser.</param>
        public async Task ProcessRequest(ProcessRequest request)
        {
            request.LogInfo($"Browser (ID: {ID}, Type: {Type}, Version {Version}) received request.");

            request.SetStatus(State.Queued, $"Waiting for lock on Context");
            await _contextLimit.WaitAsync(request.CancelToken);
            request.SetStatus(State.Processing, $"Lock aquired");

            Context context;
            try
            {

                context = await _contextFactory.CreateContext(this, request);

            }
            catch (LaunchException e)
            {
                _logger.LogError($"{e.Message}\nError:{e.InnerException}");
                throw;
            }

            try
            {
                await context.ProcessRequest();

            }
            finally
            {
                await context.CloseAsync();
                _contextLimit.Release();
            }


        }

        /// <summary>
        /// Retrieves the next unique context ID for context instances
        /// </summary>
        /// <returns>The next unique context ID</returns>
        public int GetNextContextID()
        {
            return Interlocked.Increment(ref _nextContextID);
        }

        /// <summary>
        /// Increment the total # of processing requests
        /// Called whenever a new request is received
        /// </summary>
        public void IncrementRequestCount(IClientRequest request)
        {
            int value = Interlocked.Increment(ref _requestCount); // Used to ensure value is unchanged
            _logger.LogInformation($"{request.GetType().Name} (ID: {request.ID}) received. " +
                $"-- Total Requests Active: '{value}'");
        }

        /// <summary>
        /// Decrement the total # of processing requests 
        /// Called whenever a request is terminated
        /// </summary>
        public void DecrementRequestCount(IClientRequest request)
        {
            int value = Interlocked.Decrement(ref _requestCount);
            _logger.LogInformation($"Terminating {request.GetType().Name} (ID: {request.ID}, Status: {request.State}). " +
                $"-- Total Requests Active: '{value}'");
        }

        /// <summary>
        /// Retrieves the current request count.
        /// </summary>
        /// <returns>The current request count.</returns>
        public int GetRequestCount()
        {
            return _requestCount;
        }

        /// <summary>
        /// Closes the browser instance. This should only be called once all contexts associated with this browser have been closed.
        /// </summary>
        /// <exception cref="Exception">Thrown if the browser cannot be closed.</exception>
        public async Task CloseAsync()
        {

            try
            {
                _logger.LogInformation($"Closing Browser...");
                await Instance!.CloseAsync();
                _logger.LogInformation($"Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) closed successfully.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to close Browser (ID: {ID}, Type: {Type}, Version: {Version}) due to {e}");
                _logger.LogInformation($"Disposing...");
                await Instance!.DisposeAsync();
                _logger.LogInformation($"Disposing Complete");
            }
            finally
            {
                _logger.Flush();
            }
        }

        /*
         * AutoUpdater must be implemented for Version Handling
         */

        /// <summary>
        /// Creates a browser instance based on the specified browser type and version.
        /// </summary>
        /// <param name="playwright">The parent Playwright instance used to launch the browser.</param>
        /// <param name="type">The type of the browser (e.g., "chrome", "firefox").</param>
        /// <param name="version">The version of the browser to launch.</param>
        /// <returns>The created browser instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the browser type is unsupported.</exception>
        private async Task<IBrowser> CreateBrowserInstance(IPlaywright playwright, string type, string version)
        {
            /* "Automation/Browsers/{type}-{version}/{type}-win64/{type}.exe"
                all lowercase for types. No subversions (just main version for now)

                Chrome: Automation/Browsers/chrome-{version}/chrome-win64/chome.exe
                Edge:   Ignore -> Playwright already uses latest
                Firefox:Automation/Browsers/firefox-{version}/firefox.exe
                Safari: Ignore -> Playwright already uses latest                
             
             */
            switch (type.ToLower())
            {
                case "chrome":
                    return await LaunchChrome(playwright, version);
                case "edge":
                    return await LaunchEdge(playwright, version);
                case "firefox":
                    return await LaunchFirefox(playwright, version);
                case "safari":
                    return await LaunchWebKit(playwright, version);
                default:
                    throw new ArgumentException($"Unsupported browser type: {type}");
            }
        }

        private async Task<IBrowser> LaunchChrome(IPlaywright playwright, string version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = _settings.InitializationTimeout,
                Channel = "chrome",
                Args = new[]
                {
                    "--auth-server-allowlist='*'"
                }
                /*Args = new[]
                    {
                        "--disable-cache",            // Disable cache
                        "--no-default-browser-check", // Disable default browser check
                        "--no-first-run",             // Skip first run tasks
                        "--disable-default-apps",     // Disable default apps
                        "--disable-sync",             // Disable sync to avoid shared data
                        "--disable-extensions",       // Disable extensions
                        "--disable-popup-blocking",    // Disable popup blocking
                        "--incognito"
                    }*/
                // may need for iframes: --disable-site-isolation-trials
            };
            return await playwright.Chromium.LaunchAsync(options);
        }

        private async Task<IBrowser> LaunchEdge(IPlaywright playwright, string version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = _settings.InitializationTimeout,
                Channel = "msedge",
                Args = new[]
                {
                    "--auth-server-allowlist='*'"
                }
            };
            return await playwright.Chromium.LaunchAsync(options);
        }

        private async Task<IBrowser> LaunchFirefox(IPlaywright playwright, string version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = _settings.InitializationTimeout,
                Channel = "firefox"
            };
            return await playwright.Firefox.LaunchAsync(options);
        }

        private async Task<IBrowser> LaunchWebKit(IPlaywright playwright, string version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = _settings.InitializationTimeout,
                Channel = "webkit"
            };
            return await playwright.Webkit.LaunchAsync(options);
        }
    }
}