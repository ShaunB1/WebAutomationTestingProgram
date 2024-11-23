using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;

namespace AutomationTestingProgram.Models.Backend
{
    public class Browser
    {
        private static int NextBrowserID = 0;

        public IPlaywright Parent { get; }
        public IBrowser? Instance { get; private set; }
        public int ID { get; }
        public string Type { get; }
        public int Version { get; }
        public string FolderPath { get; }
        public ContextManager? ContextManager { get; private set; }

        private int NextContextID = 0;
        private readonly ILogger<Browser> Logger;
        private readonly int Timeout;

        public Browser(IPlaywright playwright, string Type, int Version)
        {
            this.Parent = playwright;
            this.ID = Interlocked.Increment(ref NextBrowserID);
            this.NextContextID = 0;
            this.Type = Type;
            this.Version = Version;
            this.FolderPath = LogManager.CreateBrowserFolder(ID, Type, Version.ToString());

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Browser>()!;

            this.Timeout = 20000;
        }

        public async Task InitializeAsync()
        {
            try
            {
                this.Instance = await CreateBrowserInstance(this.Parent, this.Type, this.Version);
                this.ContextManager = new ContextManager(this);
                Logger.LogInformation($"Successfully initialized Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) due to {e}");
                throw new Exception($"Failed to initialize Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version})", e);
            }
        }

        public int GetNextContextID()
        {
            return Interlocked.Increment(ref NextContextID);
        }

        public async Task CreateAndRunContextAsync()
        {
            try
            {
                if (ContextManager != null)
                {
                    await ContextManager.CreateNewContextAsync();
                }
                else
                {
                    throw new ArgumentNullException($"Context Manager for Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) is null!" +
                        " Please make sure you call InitializeAsync() before use.");
                }
                
            }
            catch (Exception e)
            {
                Logger.LogError($"Browser-Level Error encountered\n {e}");
            }
        }

        public async Task CloseAsync()
        {
            try
            {
                if (Instance != null)
                {
                    await Instance.CloseAsync();
                    Logger.LogInformation($"Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) closed successfully.");
                }

                if (Logger is CustomLogger<Browser> customLogger)
                {
                    customLogger.Flush();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to close Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version}) due to {e}");
                if (Logger is CustomLogger<Browser> customLogger)
                {
                    customLogger.Flush();
                }
                throw new Exception($"Failed to close Browser (ID: {this.ID}, Type: {this.Type}, Version: {this.Version})", e);
            }
        }

        /* AutoUpdater must be written before version can be used.
         * Once implemented, update below functions to handle version.
         */
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
