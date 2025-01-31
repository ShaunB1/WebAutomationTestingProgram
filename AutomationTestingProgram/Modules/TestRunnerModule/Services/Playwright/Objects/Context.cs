using AutomationTestingProgram.Core;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class Context
    {
        /// <summary>
        /// The current instance of the context, which is created and managed by Browser Parent.
        /// </summary>
        public IBrowserContext? Instance { get; private set; }
        
        /// <summary>
        /// A unique identifier for this Context object, specific to the parent Browser instance.  
        /// Note: This ID is not globally unique across all contexts, just those with the same parent. 
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// The filepath where the context's folder is located, including logs
        /// and other related data.
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// The request linked to the Context
        /// </summary>
        public ProcessRequest Request { get; }


        /// <summary>
        /// The parent Browser instance that this Context object belongs to. 
        /// </summary>
        private Browser _parent { get; }

        /// <summary>
        /// Settings used for Context Creation/Page Management
        /// </summary>
        private ContextSettings _settings { get; }

        /// <summary>
        /// PageFactory used to create Page Objects
        /// </summary>
        private IPageFactory _pageFactory { get; }

        /// <summary>
        /// The factory used to create executor instances to run tests. 
        /// </summary>
        private IPlaywrightExecutorFactory _executorFactory { get; }

        /// <summary>
        /// Int limiting total # of active pages. Used in conjunction with lock
        /// </summary>
        private int _pageLimit { get; set; }
        private object _pageLimitLock = new object();
        
        /// <summary>
        /// Keeps track of the next unique identifier for page instances created by this object.
        /// </summary>
        private int _nextPageID;

        /// <summary>
        /// The Logger object associated with this class.
        /// </summary>
        private readonly ICustomLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// This constructor does not launch of initialize the context, it only sets up the basic properties.
        /// Please call InitializeAsync() to finish set-up.
        /// </summary>
        /// <param name="browser">Browser (parent) instance </param>
        public Context(Browser browser, ProcessRequest request, IOptions<ContextSettings> options, ICustomLoggerProvider provider, IPageFactory pageFactory, IPlaywrightExecutorFactory executorFactory)
        {
            ID = browser.GetNextContextID();
            FolderPath = LogManager.CreateContextFolder(browser.FolderPath, ID);
            Request = request;

            _parent = browser;
            _settings = options.Value;
            _pageFactory = pageFactory;
            _executorFactory = executorFactory;
            _pageLimit = _settings.PageLimit;
            _logger = provider.CreateLogger<Context>(FolderPath);
        }

        /// <summary>
        /// Initializes the context by creating a new context instance.
        /// This method must be called after the context object has been created.
        /// </summary>
        /// <exception cref="LaunchException">Thrown if the context cannot be initialized.</exception>
        public async Task InitializeAsync()
        {
            try
            {
                if (_parent.Instance == null)
                    throw new Exception("Browser instance is null when trying to initialize Context");

                Instance = await CreateContextInstanceAsync(_parent.Instance);
                _logger.LogInformation($"Successfully initialized Context (ID: {this.ID})");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to initialize Context (ID: {ID}) due to {e}");
                throw new LaunchException($"Failed to initialize Context (ID: {ID}).", e);
            }
        }

        /// <summary>
        /// Creates and runs a page from this context object.
        /// </summary>
        /// <returns></returns>
        public async Task ProcessRequest()
        {
            Request.LogInfo($"Context (ID: {ID}) received request.");

            Request.IsCancellationRequested();

            Request.LogInfo($"Linking Request and Context Folder");
            
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    LogManager.MapRequestToContextFolders(Request.FolderPath, FolderPath);
                    Request.LogInfo($"Link successfull created");
                }
                else
                {
                    Request.LogInfo($"Link not created because server is not running on windows");
                }
            }
            catch (Exception e)
            {
                Request.LogInfo($"Link failed creation due to {e}");
            }

            Page page;
            try
            {
                Request.LogInfo($"Starting Test Execution");
                page = await _pageFactory.CreatePage(this);
            }
            catch (LaunchException e)
            {
                _logger.LogError($"{e.Message}\nError:{e.InnerException}");
                throw;
            }
            
            try
            {
                IPlaywrightExecutor executor = _executorFactory.CreateExecutor(this);

                await executor.ExecuteTestFileAsync(page);
                
                Request.LogInfo($"Test Execution Successful");
            }
            finally
            {
                await page.CloseObjectAsync();
            }
        }

        /// <summary>
        /// Retrieves the next unique page ID for page instances.
        /// </summary>
        /// <returns>The next unique page ID</returns>
        public int GetNextPageID()
        {
            return Interlocked.Increment(ref _nextPageID);
        }

        /// <summary>
        /// Refreshes the Context instance.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAsync()
        {
            try
            {
                _logger.LogInformation($"Context refresh request received. Refreshing...");
                await Instance!.CloseAsync();
                _logger.LogInformation($"Context refreshed successfully.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to close Context Instance during refresh due to {e}");
                _logger.LogInformation($"Disposing...");
                await Instance!.DisposeAsync();
                _logger.LogInformation($"Disposing Complete");
            }

            try
            {
                Instance = await CreateContextInstanceAsync(_parent.Instance!);
                _logger.LogInformation($"Successfully re-initialized Context Instance");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to re-initialize Context Instance due to {e}");
                throw new LaunchException($"Failed to re-initialize Context Instance.", e);
            }
        }

        /// <summary>
        /// Closes the context object. SHOULD ONLY BE CALLED WHEN TEST EXECUTION IS FINISHED (FAILURE, COMPLETE, CANCELLED)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task CloseAsync()
        {
            try
            {
                _logger.LogInformation($"Closing Context...");
                await Instance!.CloseAsync();
                _logger.LogInformation($"Context (ID: {this.ID}) closed successfully.");

            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to close Context (ID: {ID}) due to {e}");
                _logger.LogInformation($"Disposing...");
                await Instance!.DisposeAsync();
                _logger.LogInformation($"Disposing Complete");
            }
            finally
            {
                _logger.Flush();
            }
        }

        private async Task<IBrowserContext> CreateContextInstanceAsync(IBrowser browser)
        {
            var options = new BrowserNewContextOptions
            {
                AcceptDownloads = true // Allow downloads
            };

            IBrowserContext context = await browser.NewContextAsync(options);

            /*context.Page += async (_, page) =>
            { 
                // SetupPageDownloadHandler(page);
            };*/

            return context;
        }
    }
}