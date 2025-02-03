using AutomationTestingProgram.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.DeviceManagement.Reports.RetrieveDeviceAppInstallationStatusReport;
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
                return _pages?.Last();
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
        /// Globally unique index to differentiate between downloaded files
        /// IF they have the same SuggesstedFileName.
        /// </summary>
        private int _downloadIndex;

        /// <summary>
        /// The Logger object associated with this class.
        /// </summary>
        private readonly ICustomLogger _logger;

        /// <summary>
        /// Hub Context used to log to Signal R
        /// </summary>
        private readonly IHubContext<TestHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// This constructor does not launch of initialize the page, it only sets up the basic properties.
        /// Please call InitializeAsync() to finish set-up.
        /// </summary>
        /// <param name="context">Context (parent) instance </param>
        public Page(Context context, IOptions<PageSettings> options, ICustomLoggerProvider provider, IHubContext<TestHub> hubContext)
        {
            ID = context.GetNextPageID();
            FolderPath = LogManager.CreatePageFolder(context.FolderPath, ID);

            _parent = context;
            _parent.Instance!.Page += OnPageCreated!;

            _settings = options.Value;
            _downloadIndex = 0;
            _logger = provider.CreateLogger<Page>(FolderPath);


            _index = -1;
            _pages = new List<IPage>();

            _hubContext = hubContext;

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
                await LogInfo($"Initializing Page Instance...");
            }
            else
            {
                await LogInfo($"Refreshing Page Object - Closing all open instances");

                foreach (IPage page in _pages)
                {
                    try
                    {
                        await page.CloseAsync();
                    }
                    catch (Exception e)
                    {
                        await LogError($"Error closing page: {e}");
                    }                    
                }
            }

            _index = 0;
            _pages.Clear();

            try
            {
                if (_parent.Instance == null)
                    throw new Exception("Context instance is null when trying to initialize Page");

                await CreatePageInstance(_parent.Instance, url);
                await LogInfo($"Successfully initialized Page Instance with url {url}");
            }
            catch (Exception e)
            {
                await LogError($"Failed to initialize Page Instance with url {url} due to {e}");
                throw new LaunchException($"Failed to initialize Page instance.", e);
            }
        }

        /// <summary>
        /// Closes the current page instance
        /// </summary>
        /// <returns></returns>
        public async Task CloseCurrentAsync()
        {
            try
            {
                await LogInfo($"Closing Page...");

                if (_pages.Count > 0)
                {
                    await Instance!.CloseAsync();
                    _pages.RemoveAt(_pages.Count - 1);

                    await LogInfo($"Page Instance closed successfully.");
                }
                else
                {
                    throw new Exception("No page to close");
                }

            }
            catch (Exception e)
            {
                await LogError($"Failed to close Page (ID: {ID}) due to {e}");
            }
        }

        /// <summary>
        /// Closes the Page Object
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task CloseObjectAsync()
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
                _pages.Clear();
                _logger.Flush();
            }
        }

        public async Task LogInfo(string message)
        {
            _logger.LogInformation(message);
            await _hubContext.Clients.Group(_parent.Request.ID).SendAsync("BroadcastLog", _parent.Request.ID, message);
        }

        public async Task LogWarning(string message)
        {
            _logger.LogWarning(message);
            await _hubContext.Clients.Group(_parent.Request.ID).SendAsync("BroadcastLog", _parent.Request.ID, message);
        }

        public async Task LogError(string message)
        {
            _logger.LogError(message);
            await _hubContext.Clients.Group(_parent.Request.ID).SendAsync("BroadcastLog", _parent.Request.ID, message);
        }

        public async Task LogCritical(string message)
        {
            _logger.LogCritical(message);
            await _hubContext.Clients.Group(_parent.Request.ID).SendAsync("BroadcastLog", _parent.Request.ID, message);
        }

        /// <summary>
        /// Use when you need a logging delegate. Must pass LogLevel.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Log(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    await LogCritical(message); break;
                case LogLevel.Error:
                    await LogError(message); break;
                case LogLevel.Warning:
                    await LogWarning(message); break;
                case LogLevel.Information:
                    await LogInfo(message); break;
                default:
                    throw new NotImplementedException($"Log level not implemented: {level.ToString()}");
            }
        }

        public string RetrieveDownloadFolder()
        {
            return Path.Combine(FolderPath, LogManager.DownloadPath);
        }

        public string RetrieveResultsFolder()
        {
            return Path.Combine(FolderPath, LogManager.ResultsPath);
        }

        public string RetrieveScreenShotFolder()
        {
            return Path.Combine(FolderPath, LogManager.ScreenShotPath);
        }

        public string RetrieveTempFolder()
        {
            return Path.Combine(FolderPath, LogManager.TempFilePath);
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

        private void OnPageCreated(object sender, IPage page)
        {
            _pages.Add(page);
            
            page.Download += async (_, download) =>
            {
                var downloadPath = Path.Combine(RetrieveDownloadFolder(), Interlocked.Increment(ref _downloadIndex) + "_" + download.SuggestedFilename);

                await download.SaveAsAsync(downloadPath);

                await LogInfo($"Downloaded file: {download.SuggestedFilename}");
            };
        }
       
    }
}