using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;

namespace AutomationTestingProgram.Models.Backend
{
    public class Context
    {
        private static int NextContextID = 0;

        public IBrowser Parent { get; }
        public IBrowserContext? Instance { get; private set; }
        public int ID { get; }
        public string FolderPath { get; }
        public PageManager? PageManager { get; private set; }

        private readonly ILogger<Context> Logger;

        public Context(IBrowser browser, string BrowserFolderPath)
        {
            this.Parent = browser;
            this.ID = Interlocked.Increment(ref NextContextID);
            this.FolderPath = LogManager.CreateContextFolder(BrowserFolderPath, ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Context>()!;
        }

        public async Task InitializeAsync()
        {
            try
            {
                this.Instance = await CreateContextInstance(this.Parent);
                this.PageManager = new PageManager(this.Instance);
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize context: {this.ID} because of\n{e}");
            }
        }

        private async Task<IBrowserContext> CreateContextInstance(IBrowser browser)
        {
            var options = new BrowserNewContextOptions
            {
                
            };
            return await browser.NewContextAsync(options);
        }

    }
}
