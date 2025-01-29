using AutomationTestingProgram.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

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
                return _pages?.ElementAt(_index);
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
        /// The current index of the active IPage in the Pages List.
        /// Only -1 first time.
        /// </summary>
        private int _index;

        /// <summary>
        /// Page list containing all active IPages within the Page object
        /// Note: This means other tabs/windows.
        /// </summary>
        private IList<IPage> _pages;
        
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


            _index = -1;
            _pages = new List<IPage>();

        }

        /// <summary>
        /// Initializes/re-initializes the Page Object.
        /// Will close all Active Pages (if any), and reset the _index.
        /// Creates a new page instance at the specified url.
        /// </summary>
        /// <exception cref="LaunchException">Thrown if the page cannot be initialized.</exception>
        public async Task RefreshAsync(string url)
        {
            if (_index == -1)
            {
                _logger.LogInformation($"Initializing Page Instance...");
            }
            else
            {
                _logger.LogInformation($"Refreshing Page Object - Closing all open instances");

                foreach (IPage page in _pages)
                {
                    try
                    {
                        await page.CloseAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation($"Error closing page: {e}");
                    }                    
                }
            }

            _index = 0;
            _pages.Clear();

            try
            {
                if (_parent.Instance == null)
                    throw new Exception("Context instance is null when trying to initialize Page");

                _pages.Add(await CreatePageInstance(_parent.Instance, url));
                _logger.LogInformation($"Successfully initialized Page Instance with url {url}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to initialize Page Instance with url {url} due to {e}");
                throw new LaunchException($"Failed to initialize Page instance.", e);
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
                foreach (IPage page in _pages)
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

        public void LogInfo(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
                _logger.LogWarning(message);
        }

        public void LogError(string message)
        {
                _logger.LogError(message);
        }

        public void LogCritical(string message)
        {
                _logger.LogCritical(message);
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