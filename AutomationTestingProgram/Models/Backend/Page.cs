using AutomationTestingProgram.Services.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;

namespace AutomationTestingProgram.Models.Backend
{
    public class Page
    {
        private static int NextPageID = 0;

        public IBrowserContext Parent { get; }
        public IPage? Instance
        {
            get
            {
                return Pages?.ElementAt(InstanceIndex);
            }
        }
        public int InstanceIndex { get; private set; }
        public string? Url
        {
            get
            {
                return Instance?.Url;
            }
        }
        public List<IPage> Pages { get; private set; }
        public int ID { get; }
        public string FolderPath { get; }

        private readonly ILogger<Page> Logger;

        public Page(IBrowserContext browserContext, string contextFolderPath)
        {
            this.Parent = browserContext;
            this.ID = Interlocked.Increment(ref NextPageID);
            this.FolderPath = LogManager.CreatePageFolder(contextFolderPath, ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Page>()!;        }

        public async Task InitializeAsync()
        {
            try
            {                
                this.InstanceIndex = 0;
                this.Pages = new List<IPage>() { await CreatePageInstance(this.Parent) };
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize page: {this.ID} because of\n{e}");
            }
        }

        private async Task<IPage> CreatePageInstance(IBrowserContext browserContext)
        {
            return await browserContext.NewPageAsync();
        }

        public async AddNewPage
        public async RemovePage
        public async SwitchPage

        public async Task<Page> CopyAsync()
        {
            try
            {                
                var newPage = new Page(this.Parent, LogManager.GetContextFolder(this.FolderPath));
                await newPage.InitializeAsync();

                // Copying all the urls from the original page
                foreach (var page in this.Pages)
                {
                    // We create new IPage instances for each URL, using the same browser context.
                    var newPageInstance = await this.Parent.NewPageAsync();

                    // Here we would need a method to copy or navigate to the same URL from the original page.
                    var url = page.Url;  // Assuming you want to navigate to the same URL
                    await newPageInstance.GotoAsync(url);

                    newPage.Pages.Add(newPageInstance);
                }

                return newPage;
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to copy page: {this.ID} because of\n{e}");
                throw;
            }
        }


    }
}
