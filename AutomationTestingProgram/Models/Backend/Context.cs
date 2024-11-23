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
        public Browser Parent { get; }
        public IBrowserContext? Instance { get; private set; }
        public int ID { get; }
        public string FolderPath { get; }
        public PageManager? PageManager { get; private set; }

        private readonly ILogger<Context> Logger;
        private int NextPageID = 0;

        public Context(Browser browser)
        {
            this.Parent = browser;
            this.ID = browser.GetNextContextID();
            this.FolderPath = LogManager.CreateContextFolder(browser.FolderPath, ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Context>()!;
        }

        public async Task InitializeAsync()
        {
            try
            {
                this.Instance = await CreateContextInstance(this.Parent);
                this.PageManager = new PageManager(this);
                Logger.LogInformation($"Successfully initialized Context (ID: {this.ID})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize Context (ID: {this.ID}) because of\n{e}");
                throw new Exception($"Failed to initialize Context (ID: {this.ID})", e);
            }
        }

        public int GetNextPageID()
        {
            return Interlocked.Increment(ref NextPageID);
        }

        public async Task CreateAndRunPageAsync()
        {
            try
            {
                if (PageManager != null)
                {
                    await PageManager.CreateNewPrimaryPageAsync();
                }
                else
                {
                    throw new ArgumentNullException($"Page Manager for Context (ID: {this.ID}) inside of Browser (ID: {Parent.ID}, Type: {Parent.Type}, Version: {Parent.Version}) is null!" +
                        $" Please make sure you call InitializeAsync() before use.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Context-Level Error encountered\n {e}");
            }
        }

        public async Task CloseAsync()
        {
            try
            {
                if (Instance != null)
                {
                    await Instance.CloseAsync();
                    Logger.LogInformation($"Context (ID: {this.ID}) closed successfully.");
                }

                if (Logger is CustomLogger<Context> customLogger)
                {
                    customLogger.Flush();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to close Context (ID: {this.ID}) due to {e}");
                if (Logger is CustomLogger<Context> customLogger)
                {
                    customLogger.Flush();
                }
                throw new Exception($"Failed to close Context (ID: {this.ID})", e);
            }
        }

        private async Task<IBrowserContext> CreateContextInstance(Browser browser)
        {   
            if (browser?.Instance == null) 
            {
                throw new ArgumentNullException($"Context (ID: {this.ID}) cannot be created because" +
                    $" Browser (ID: {Parent.ID}, Type: {Parent.Type}, Version: {Parent.Version})" +
                    $" instance is null!");
            }

            var options = new BrowserNewContextOptions
            {
                
            };
            return await browser.Instance.NewContextAsync(options);
        }

    }
}
