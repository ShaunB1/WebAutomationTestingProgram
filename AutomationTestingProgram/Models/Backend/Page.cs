using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Services.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;

namespace AutomationTestingProgram.Models.Backend
{
    public class Page
    {
        public Context Parent { get; }
        public IPage? Instance
        {
            get
            {
                return Pages.ElementAt(Index);
            } 
        }
        public int Index { get; private set; }
        public List<IPage>? Pages { get; private set; }
        public string url
        {
            get
            {
                return Instance?.Url ?? "";
            }
        }
        public int ID { get; }
        public string FolderPath { get; }

        private readonly ILogger<Page> Logger;

        public Page(Context context)
        {
            this.Parent = context;
            this.ID = context.GetNextPageID();
            this.FolderPath = LogManager.CreatePageFolder(context.FolderPath, ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Page>()!;
        }

        public async Task InitializeAsync()
        {
            try
            {
                this.Index = 0;
                this.Pages = new List<IPage>() { await CreatePageInstance(this.Parent) };
                Logger.LogInformation($"Successfully initialized Page (ID: {this.ID})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize Page (ID: {this.ID}) because of\n{e}");
                throw new Exception($"Failed to initialize Page (ID: {this.ID})", e);
            }
        }

        public async Task InitializeAsync(Page pageObject)
        {
            try
            {
                this.Index = pageObject.Index;
                this.Pages = new List<IPage>();

                foreach (IPage page in pageObject.Pages)
                {
                    this.Pages.Add(await CreatePageInstance(this.Parent, page.Url));
                }

                Logger.LogInformation($"Successfully initialized a copied Page (ID: {this.ID}) from Page (ID: {pageObject.ID})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize a copied Page (ID: {this.ID}) from Page (ID: {pageObject.ID}) because of\n{e}");
                throw new Exception($"Failed to initialize a copied Page (ID: {this.ID}) from Page (ID: {pageObject.ID})", e);
            }
        }

        public void SwitchToPage(int index)
        {
            this.Index = index;
        }

        public async Task RunAsync()
        {
            try
            {
                if (Instance != null)
                {
                    await FileReader.ExecutePage(this);
                }
                else
                {
                    throw new ArgumentNullException($"Page Instance for Page (ID: {this.ID}) inside of Context (ID: {Parent.ID}) is null!" +
                        $" Please make sure you call InitializeAsync() before use.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Page-Level Error encountered\n {e}");
            }
        }

        public async Task CloseAllAsync()
        {
            try
            {
                if (Pages != null)
                {
                    foreach (IPage page in Pages)
                    {
                        await page.CloseAsync();
                    }
                    Logger.LogInformation($"Page (ID: {this.ID}) closed successfully.");
                }

                if (Logger is CustomLogger<Page> customLogger)
                {
                    customLogger.Flush();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to close Page (ID: {this.ID}) due to {e}");
                if (Logger is CustomLogger<Page> customLogger)
                {
                    customLogger.Flush();
                }
                throw new Exception($"Failed to close Page (ID: {this.ID})", e);
            }
        }

        private async Task<IPage> CreatePageInstance(Context context, string url = "")
        {
            if (context?.Instance == null)
            {
                throw new ArgumentNullException($"Page (ID: {this.ID}) cannot be created because" +
                    $" Context (ID: {Parent.ID} instance is null!");
            }
            IPage page = await context.Instance.NewPageAsync();

            if (!string.IsNullOrEmpty(url))
            {
                var options = new PageGotoOptions
                {
                    Timeout = 60000
                };

                await page.GotoAsync(url, options);
            }

            return page;
        }
       
    }
}
