/*using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;

namespace AutomationTestingProgram.ModelsOLD
{
    public class Context
    {
        public Browser Parent { get; }
        private ContextManager ContextManager => Parent.ContextManager!;
        public IBrowserContext? Instance { get; private set; }
        public int ID { get; }
        *//*
         * When login/entering credentials, set current user used to do so.
         * When logout (not closing context), set to empty
         *//* 
        public string CurrentUser { get; set; }
        public string FolderPath { get; }
        public PageManager? PageManager { get; private set; }

        private readonly ILogger<Context> Logger;
        private int NextPageID = 0;

        /// <summary>
        /// The Context object.
        /// </summary>
        /// <param name="browser">Browser (parent) instance </param>
        public Context(Browser browser)
        {
            this.Parent = browser;
            this.ID = browser.GetNextContextID();
            this.CurrentUser = string.Empty;
            this.FolderPath = LogManager.CreateContextFolder(browser.FolderPath, ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(this.FolderPath);
            Logger = provider.CreateLogger<Context>()!;
        }

        /// <summary>
        /// Initializes the context object. MUST BE CALLED AFTER DEFINING THE OBJECT FROM THE CONSTRUCTOR.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task InitializeAsync()
        {
            try
            {
                this.Instance = await CreateContextInstanceAsync(this.Parent);
                this.PageManager = new PageManager(this);
                Logger.LogInformation($"Successfully initialized Context (ID: {this.ID})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize Context (ID: {this.ID}) because of\n{e}");
                throw new Exception($"Failed to initialize Context (ID: {this.ID})", e);
            }
        }

        /// <summary>
        /// Gets the id -> used by page objects created from this.PageManager
        /// </summary>
        /// <returns></returns>
        public int GetNextPageID()
        {
            return Interlocked.Increment(ref NextPageID);
        }

        /// <summary>
        /// Creates and runs a page from this context object.
        /// </summary>
        /// <returns></returns>
        public async Task ProcessRequest(Request request)
        {           
            request.SetPath(this.FolderPath);
            
            List<FileBreakpoint> breakpoints = new List<FileBreakpoint>();
            // Validation
            try
            {
                if (PageManager == null)
                {
                    throw new ArgumentNullException($"Page Manager for Context (ID: {this.ID}) inside of Browser (ID: {Parent.ID}, Type: {Parent.Type}, Version: {Parent.Version}) is null!" +
                        $" Please make sure you call InitializeAsync() before use.");
                }

                breakpoints = await ValidateFileAsync(request);
            }
            catch (Exception e)
            {
                request.SetStatus(RequestState.Validating, "Request Failed validation", e);
                await ContextManager.TerminateContextAsync(this, request);
                return;
            }

            // Processing
            
            try
            {
                request.SetStatus(RequestState.Processing, "Context Processing Request");
                
            }
            catch (Exception e)
            {
                Logger.LogError($"Context-Level Error encountered\n {e}");
                request.SetStatus(RequestState.ProcessingFailure, "Context Processing Failure", e);
            }
        }

        /// <summary>
        /// Closes the context object. SHOULD ONLY BE CALLED WHEN TEST EXECUTION IS FINISHED (FAILURE, COMPLETE, ERROR)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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

        private async Task<IBrowserContext> CreateContextInstanceAsync(Browser browser)
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
*/