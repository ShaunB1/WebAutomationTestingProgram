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


        private readonly ILogger<Browser> Logger;
        private readonly int Timeout;

        public Browser(IPlaywright playwright, string Type, int Version)
        {
            this.Parent = playwright;
            this.ID = Interlocked.Increment(ref NextBrowserID);
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
                this.ContextManager = new ContextManager(this.Instance);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize Browser: {this.ID}:{this.Type}:{this.Version} because of\n{e}");
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
                    throw new ArgumentException("Unsupported browser type");
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
