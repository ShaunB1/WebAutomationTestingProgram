using AutomationTestingProgram.Core;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Account;
using System;
using System.Collections.Generic;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class Page
    {   
        /// <summary>
        /// Retrieves the current active IPage of this Page Object.
        /// A Page object can have multiple IPages (tabs/windows), but
        /// only one is active at a time.
        /// </summary>
        public IPage? Instance
        {
            get
            {
                return Pages?.ElementAt(Index);
            }
            private set { }
        }

        /// <summary>
        /// The current URL of the current active IPage
        /// </summary>
        private string url
        {
            get
            {
                return Instance?.Url ?? "";
            }
        }

        /// <summary>
        /// The current index of the active IPage in the Pages List.
        /// -1 -> No active page
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Page list containing all active IPages within the Page object
        /// Note: This means other tabs/windows.
        /// </summary>
        public IList<IPage> Pages { get; set; }

        /// <summary>
        /// A unique identifier for this Page object, specific to the parent Context instance.
        /// Note: This ID is not globally unique across all pages, just those with the same parent.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// The filepath where the page's folder is located, including logs
        /// and other related data.
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// The parent Context instance that this Page object belongs to.
        /// </summary>
        private Context _parent { get; }
        
        /// <summary>
        /// Settings used for Page Creation/Management
        /// </summary>
        private PageSettings _settings { get; }

        /// <summary>
        /// The Logger object associated with this class.
        /// </summary>
        private readonly ICustomLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// This constructor does not launch of initialize the page, it only sets up the basic properties.
        /// Please call InitializeAsync() to finish set-up.
        /// </summary>
        /// <param name="context">Context (parent) instance </param>
        public Page(Context context, IOptions<PageSettings> options, ICustomLoggerProvider provider)
        {
            ID = context.GetNextPageID();
            FolderPath = LogManager.CreatePageFolder(context.FolderPath, ID);

            _parent = context;
            _settings = options.Value;
            _logger = provider.CreateLogger<Page>(FolderPath);

            Index = 0;
            Pages = new List<IPage>();

        }

        /// <summary>
        /// Initializes the page by creating a new page instance.
        /// This method must be called after the page object has been created.
        /// </summary>
        /// <exception cref="LaunchException">Thrown if the context cannot be initialized.</exception>
        public async Task InitializeAsync()
        {
            try
            {
                if (_parent.Instance == null)
                    throw new Exception("Context instance is null when trying to initialize Page");

                Pages.Add(await CreatePageInstance(_parent.Instance));
                _logger.LogInformation($"Successfully initialized Page (ID: {this.ID})");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to initialize Page (ID: {ID}) due to {e}");
                throw new LaunchException($"Failed to initialize Page (ID: {ID}).", e);
            }
        }

        /// <summary>
        /// Switches the current page (changes index of active page in list)
        /// </summary>
        /// <param name="index"></param>
        public void SwitchToPage(int index)
        {
            this.Index = index;
        }

        /// <summary>
        /// Runs the page object.
        /// </summary>
        /// <returns></returns>
        public async Task ProcessAsync(CancellationToken cancelToken)
        {
            try
            {
                if (Instance == null)
                {
                    throw new Exception("IPage instance is null.");
                }

                _logger.LogInformation("Starting");
                await Instance.GotoAsync("https://www.google.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Google complete");
                await Instance.GotoAsync("https://example.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Example complete");
                await Instance.GotoAsync("https://www.bing.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Bing complete");
                await Instance.GotoAsync("https://www.yahoo.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Yahoo complete");
                await Instance.GotoAsync("https://www.wikipedia.org");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Wikipedia complete");
                await Instance.GotoAsync("https://www.reddit.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Reddit complete");
                await Instance.GotoAsync("https://www.microsoft.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Microsoft complete");
                await Instance.GotoAsync("https://www.apple.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Apple complete");
                await Instance.GotoAsync("https://www.amazon.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Amazon complete");
                await Instance.GotoAsync("https://www.netflix.com");
                await Task.Delay(10000, cancelToken);
                _logger.LogInformation("Netflix complete");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError($"Request Cancelled. Stopping all page processing..");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Page-Level Error encountered\n {e}");
                throw;
            }
        }

        /// <summary>
        /// Closes all pages associated with the page object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task CloseAllAsync()
        {
            try
            {
                _logger.LogInformation($"Closing Page...");
                foreach (IPage page in Pages)
                {
                    await page.CloseAsync();
                }
                _logger.LogInformation($"Page (ID: {this.ID}) closed successfully.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to close Page (ID: {ID}) due to {e}");
            }
            finally
            {
                _logger.Flush();
            }
        }

        private async Task<IPage> CreatePageInstance(IBrowserContext context, string url = "")
        {
            IPage page = await context.NewPageAsync();

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