/*using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Backend.Managers;

namespace AutomationTestingProgram.Backend
{   
    /// <summary>
    /// Represents the Manager that manages <see cref="Context"/> objects.
    /// </summary>
    public class ContextManager
    {
        /// <summary>
        /// The Browser Parent Instance
        /// </summary>
        private readonly Browser Browser;

        /// <summary>
        /// Linkage to the Browser Manager that created the Parent of this Context Manager
        /// </summary>
        private BrowserManager BrowserManager => Browser.Parent.BrowserManager!;

        /// <summary>
        /// Allows up to '10' contexts to run concurrently at a time, per Browser Instance
        /// </summary>
        private readonly SemaphoreSlim ContextSemaphore;

        /// <summary>
        /// Queue for context creation tasks. Each task is responsible for creating a new context instance.
        /// </summary>
        private readonly ConcurrentQueue<Func<Task<Context>>> ContextQueue;

        /// <summary>
        /// The Logger Object associated with this class
        /// </summary>
        private readonly ILogger<ContextManager> Logger;

        /// <summary>
        /// Contains the total # of active requests (contexts) within the Context Manager class
        /// </summary>
        private int ActiveRequestCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextManager"/> class.
        /// This class manages <see cref="Context"/> objects.
        /// </summary>
        /// <param name="browser"></param>
        public ContextManager(Browser browser)
        {
            Browser = browser;
            ContextSemaphore = new SemaphoreSlim(10); // Limit of 10 concurrences contexts.
            ContextQueue = new ConcurrentQueue<Func<Task<Context>>>();
            ActiveRequestCount = 0;

            CustomLoggerProvider provider = new CustomLoggerProvider(browser.FolderPath);
            Logger = provider.CreateLogger<ContextManager>()!;
        }
        
        /// <summary>
        /// Processes an incoming request by queing it for context execution
        /// </summary>
        /// <param name="request">The request to be processed</param>
        /// <returns></returns>
        public async Task ProcessRequestAsync(RequestOLD request)
        {
            request.SetStatus(RequestState.Queued, "Context Manager Queued Request");
            BrowserManager.IncrementRequestCount(Browser, request);
            ContextQueue.Enqueue(CreateNewContextTask(request));
            await ProcessNextContext();
        }

        /// <summary>
        /// Creates a new task that, when executed, will create and initialize a new context instance.
        /// This context will then start processing the request.
        /// </summary>
        /// <param name="request">The request associated with the context</param>
        /// <returns>A task that creates and returns a new context</returns>
        private Func<Task<Context>> CreateNewContextTask(Request request)
        {
            Func<Task<Context>> createContext = async () =>
            {
                Context context = new Context(Browser);
                IncrementRequestCount(context, request);
                try
                {                    
                    await context.InitializeAsync();
                    ExecuteContextAsync(context, request);
                    return context;
                }
                catch (Exception e)
                {   
                    *//* Only thing in try block that could fail is InitializeAsync
                     * This would occur if too heavy a load and timeout not enough.
                     * Must test to see if an issue, and if a retry mechasim is needed.
                     * 
                     * 
                     *//*

                    Logger.LogError($"Browser-Level Error encountered\n {e}");
                    DecrementRequestCount(context, request);
                    ContextSemaphore.Release();
                    throw;
                }
            };

            return createContext;
        }

        /// <summary>
        /// Terminates a context.
        /// Called by the Context Object once a request finished processing 
        /// </summary>
        /// <param name="context">The context to terminate</param>
        /// <param name="request">The request associated with the context</param>
        /// <returns></returns>
        public async Task TerminateContextAsync(Context? context, Request request)
        {   
            await context!.CloseAsync();
            DecrementRequestCount(context, request);
            BrowserManager.DecrementRequestCount(Browser, request);
            context = null;
            ContextSemaphore.Release();
            await ProcessNextContext();

            // Check if Browser should be terminated
            if (SafeToClose())
            {
                *//* While BrowserManager and ContextManager are both thread safe for their own class,
                 * there are issues with them interacting with each other.
                 * Mainly: Multiple requests to terminate browser at a time, or receiving a new request
                 * just before we try to terminate the browser, and then the request immediatelly terminates,
                 * resulting in a race condition between a TerminateBrowserAsync releasing a lock, and TerminateContextAsync
                 * trying to aquire etc. etc. etc.
                 * 
                 * Anyways, to solve these issues, BrowserManages already implements locks to make sure multiple termination
                 * requests for the same browser occur sequentially. All is needed is a bool for the Browser object, detecting
                 * whether it is terminated or not. If terminated, we ingore next requests, solving this whole issue.
                 * This means if 10 contexts all terminate at the same time, and all pass SafeToClose(), they will all call
                 * TerminateBrowserAsync(). 
                 * 
                 * If a new request is received, taking the lock before the termination, it will start processing, and 
                 * the terminations will do nothing, as a context is now running. If the request terminates immediatelly, it will
                 * call TerminateBrowserAsync() on its own, ensuring it will terminate.
                 * 
                 * As we process through the terminates, one will eventually succeed in the closing the browser. In fact,
                 * the one from the request that immediatelly terminated is guaranteed to close it, setting the boolean to true, 
                 * ensuring future calls are ignore for the same browser. (Remember, locks make this sequential, so no concurrency issues)
                 * 
                 * If the TerminateBrowserAsync() receives the lock first, it will close the browser. Future calls to TerminateBrowserAsync()
                 * will then fail. Note: If a new request is sent, and competes with the lock with other terminates, trying to run on the same
                 * browser that was just terminated:
                 * 
                 * -> In this scenario, lets say all requests are for the same browser: chrome 123
                 * We terminate chrome 123
                 * We terminate chrome 123
                 * We send a request to chrome 123
                 * We terminate chrome 123
                 * We terminate chrome 123 
                 * 
                 * What will happen?
                 * We terminate chrome 123. The browser instance is terminated
                 * The next termination sees that IsTermianted is true, and ignores
                 * We send a request for chrome 123. We see that chrome 123 is no longer active, so we queue it, and then try to start it.
                 * It successfully starts, with chrome 123 now open again, and a new request sent.
                 * The next termination sees that IsTerminated is true, and ignores.
                 * The next termination sees that IsTerminated is true, and ignores.
                 * 
                 * Why does it not terminate the newly created chrome 123?
                 * Its a different object. When it terminates, we send a reference of the chrome we want to terminate.
                 * So the one that is suppoed to terminate is chrome 123 object A, but the newly created on is chrome 123 object B.
                 * 
                 * Once references are all removed to object A, garbage collector will perform cleanup. 
                 * 
                 *//*
                _ = BrowserManager.TerminateBrowserAsync(Browser); 
            }
                
        }

        /// <summary>
        /// Checks if it is safe to close the context manager, based on the number of requests (active or queued)
        /// </summary>
        /// <returns></returns>
        public bool SafeToClose()
        {
            return ActiveRequestCount == 0 && ContextQueue.IsEmpty;
        }

        /// <summary>
        /// Sends a request to the context object for processing. This will start the process, but not wait for completion.
        /// </summary>
        /// <param name="context">The context used to process the request</param>
        /// <param name="request">The request to process</param>
        private void ExecuteContextAsync(Context context, Request request)
        {
            try
            {
                request.SetStatus(RequestState.Processing, "Context Manager Sending Request -> Context");
                // We make sure the operation started. Results sent to task completion source
                _ = context.ProcessRequest(request); 
            }
            catch (Exception e) // Should not occur
            {
                request.SetStatus(RequestState.ProcessingFailure, "Context Manager Sending Request -> Context: Failure", e);
            }
        }

        /// <summary>
        /// Increment the total # of processing requests BROWSER LEVEL
        /// Called whenever a new request starts processing.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        private void IncrementRequestCount(Context context, Request request)
        {
            Interlocked.Increment(ref ActiveRequestCount);
            Logger.LogInformation($"New Request (ID: {request.ID}) processing in Context (ID: {context.ID}) " +
                $"-- Total Requests Processing: '{ActiveRequestCount}' | Queued: '{ContextQueue.Count}'");
        }

        /// <summary>
        /// Decrement the total # of processing requests BROWSER LEVEL
        /// Called whenever a new request completes (failure or success)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        private void DecrementRequestCount(Context context, Request request)
        {
            Interlocked.Decrement(ref ActiveRequestCount);
            Logger.LogInformation($"Terminating Request (ID: {request.ID}, Result: {request.State}) processing in Context (ID: {context.ID}) " +
                $"-- Total Requests Processing: '{ActiveRequestCount}' | Queued: '{ContextQueue.Count}'");
        }

        /// <summary>
        /// Called when a new request is received, ensuring context processing if there is available capacity.
        /// Also called when a context terminates execution, as it frees up a new slot for additional requests.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessNextContext()
        {
            if (await ContextSemaphore.WaitAsync(0))
            {
                if (ContextQueue.TryDequeue(out var nextTask))
                {
                    await Task.Run(nextTask);
                }
                else
                {
                    ContextSemaphore.Release();
                }
            }
            else if (ContextQueue.Count > 0)
            {
                Logger.LogInformation($"Queuing Context -- Contexts Running: '{ActiveRequestCount}' | Queued: '{ContextQueue.Count}'");
            }
        }
    }
}
*/