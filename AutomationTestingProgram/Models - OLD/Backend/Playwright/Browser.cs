/*using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutomationTestingProgram.ModelsOLD
{
    /// <summary>
    /// Represents a browser object that manages browser instances, contexts, and related tasks.
    /// </summary>
    public class Browser
    {
        /// <summary>
        /// The parent Playwright instance that this Browser object belongs to.
        /// </summary>
        public PlaywrightObject Parent { get; }

        /// <summary>
        /// The current instance of the browser, which is created and managed by Playwright.
        /// </summary>
        public IBrowser? Instance { get; private set; }

        /// <summary>
        /// Bool detecting whether a Browser Object is in a 'Terminated' state.
        /// Used for concurrency issues
        /// </summary>
        public bool IsTerminated = false;

        /// <summary>
        /// A unique identifier for this Browser object, specific to the parent Playwright instance.
        /// Note: This ID is not globally unique across all browsers, just those with the same parent.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// The Type of the Browser
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The Version of the Browser
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// The filepath where the browser's folder is located, including logs and other related data
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// Manages context instances created by this object
        /// </summary>
        public ContextManager? ContextManager { get; private set; }

        /// <summary>
        /// Keeps track of the next unique idetifier for browser instances created by this object
        /// </summary>
        private int NextContextID;

        /// <summary>
        /// The Logger object associated with this class
        /// </summary>
        private readonly ILogger<Browser> Logger;

        /// <summary>
        /// The timeout for creating a new browser instance.
        /// </summary>
        private readonly int Timeout = 20000; // Should be able to be changed

        /// <summary>
        /// Initializes a new instance of the <see cref="Browser"/> class.
        /// This constructor does not launch or initialize the browser, it only sets up the basic properties.
        /// Please call InitializeAsync() right after the constructor.
        /// </summary>
        /// <param name="playwright">The parent Playwright object that manages the browser.</param>
        /// <param name="Type">The type of the browser (e.g., "chrome", "firefox").</param>
        /// <param name="Version">The version of the browser (e.g., 91, 92).</param>
        public Browser(PlaywrightObject playwright, string Type, int Version)
        {
            this.Parent = playwright;
            this.ID = playwright.GetNextBrowserID();
            this.NextContextID = 0;
            this.Type = Type;
            this.Version = Version;
            this.FolderPath = LogManager.CreateBrowserFolder(ID, Type, Version.ToString());

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Browser>()!;
        }

        /// <summary>
        /// Initializes the browser by creating a new browser instance and setting up its context manager.
        /// This method must be called after the browser object has been created.
        /// </summary>
        /// <exception cref="Exception">Thrown if the browser cannot be initialized.</exception>
        public async Task InitializeAsync()
        {
            try
            {
                this.Instance = await CreateBrowserInstance(this.Parent.Instance, this.Type, this.Version);
                // this.ContextManager = new ContextManager(this);
                Logger.LogInformation($"Successfully initialized Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) due to {e}");
                throw new Exception($"Failed to initialize Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version})", e);
            }
        }

        /// <summary>
        /// Retrieves the next unique context ID for context instances
        /// </summary>
        /// <returns>The next unique context ID</returns>
        public int GetNextContextID()
        {
            return Interlocked.Increment(ref NextContextID);
        }

        /// <summary>
        /// Sends a request to the Browser Manager for processing. This will initiate the request, but does not wait for completion.
        /// </summary>
        /// <param name="request">The request to process within the browser.</param>
        public async Task ProcessRequest(Request request)
        {
            try
            {
                request.SetStatus(RequestState.Processing, "Browser Sending Request -> Context Manager");

                if (ContextManager == null)
                {
                    throw new ArgumentNullException($"Context Manager for Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) is null!" +
                        " Please make sure you call InitializeAsync() before use.");
                }

                // Just makes sure the context was successfully created/queued. Results in request TaskCompletionSource
                await ContextManager.ProcessRequestAsync(request);

            }
            catch (Exception e)
            {
                Logger.LogError($"Browser-Level Error encountered\n {e}");
                request.SetStatus(RequestState.ProcessingFailure, "Browser sending Request -> Context Manager: Failure", e);
            }
        }

        /// <summary>
        /// Closes the browser instance. This should only be called once all contexts associated with this browser have been closed.
        /// </summary>
        /// <exception cref="Exception">Thrown if the browser cannot be closed.</exception>
        public async Task<bool> CloseAsync()
        {
            bool closed = false;

            try
            {   
                // No active or queued requests, close browser instance
                if (ContextManager!.SafeToClose())
                {
                    await Instance!.CloseAsync();
                    Instance = null;
                    closed = true;
                    Logger.LogInformation($"Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) closed successfully.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to close Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) due to {e}");
                
                throw new Exception($"Failed to close Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version})", e);
            }
            finally
            {
                if (Logger is CustomLogger<Browser> customLogger)
                {
                    customLogger.Flush();
                }
            }

            return closed;
        }

        *//* AutoUpdater must be written before version can be used.
         * Once implemented, update below functions to handle version.
         *//*

        /// <summary>
        /// Creates a browser instance based on the specified browser type and version.
        /// </summary>
        /// <param name="playwright">The parent Playwright instance used to launch the browser.</param>
        /// <param name="type">The type of the browser (e.g., "chrome", "firefox").</param>
        /// <param name="version">The version of the browser to launch.</param>
        /// <returns>The created browser instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the browser type is unsupported.</exception>
        private async Task<IBrowser> CreateBrowserInstance(IPlaywright playwright, string type, int version)
        { 
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

        private async Task<IBrowser> LaunchChrome(IPlaywright playwright, int version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = this.Timeout,
                Channel = "chrome"
            };
            return await playwright.Chromium.LaunchAsync(options);
        }

        private async Task<IBrowser> LaunchEdge(IPlaywright playwright, int version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = this.Timeout,
                Channel = "msedge"
            };
            return await playwright.Chromium.LaunchAsync(options);
        }

        private async Task<IBrowser> LaunchFirefox(IPlaywright playwright, int version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = this.Timeout,
                Channel = "firefox"
            };
            return await playwright.Firefox.LaunchAsync(options);
        }

        private async Task<IBrowser> LaunchWebKit(IPlaywright playwright, int version)
        {
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                Timeout = this.Timeout,
                Channel = "webkit"
            };
            return await playwright.Webkit.LaunchAsync(options);
        }
    }
}
*/