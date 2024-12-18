using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend.Request
{   
    /// <summary>
    /// The Request Handler handles all incoming requests, including validation and processing.
    /// </summary>
    public static class RequestHandler
    {   
        /// <summary>
        /// Only a given amount of requests can be active at a time. Rest are queued.
        /// </summary>
        private static readonly SemaphoreSlim ActiveRequests = new SemaphoreSlim(50);

        private static readonly ConcurrentQueue<Func<Task>> RequestQueue = new ConcurrentQueue<Func<Task>>();

        /*
         * 1. See if Program.cs can limit # of requests.
         * 2. Re-look at all request logic
         * 3. Re-look at logManager logic
         * 4. Request Handler copy of BrowserManager. Up to 50 active requests, rest are queued.
         * 5. Requests first validate, then execute (depending on the type)
         * ** TEST IN STEPS *** 
         * 
         */
    }
}
