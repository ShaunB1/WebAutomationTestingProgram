using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend
{   
    /// <summary>
    /// The Request Handler handles all incoming requests.
    /// </summary>
    public static class RequestHandler
    {   
        /// <summary>
        /// The Playwright Object used to precess requests using playwright automation
        /// </summary>
        public static readonly PlaywrightObject Playwright;

        /// <summary>
        /// Dictionary of all active requests, keyed by their id
        /// </summary>
        private static readonly ConcurrentDictionary<string, IClientRequest> _requests;

        /// <summary>
        /// Flag detecting whether the application is shutting down. Used to prevent new requests
        /// from being received.
        /// </summary>
        private static bool ShutdownFlag = false;
        private static readonly object ShutdownLock = new object();

        static RequestHandler()
        {
            Playwright = new PlaywrightObject();
            _requests = new ConcurrentDictionary<string, IClientRequest>();
        }

        /// <summary>
        /// Processes a given request, and returns its result
        /// </summary>
        /// <param name="request">The request to process</param>
        /// <returns>The result of the request</returns>
        public static async Task ProcessRequestAsync(IClientRequest request)
        {
            lock (ShutdownLock)
            {
                if (ShutdownFlag)
                {
                    try 
                    {
                        request.CancellationTokenSource.Cancel();
                    }
                    catch
                    {
                        // Can only fail if request already cancelled.
                    }
                }

                _requests.TryAdd(request.ID, request);
            }

            request.SetStatus(State.Received, $"{request.GetType().Name} (ID: {request.ID}) received.");

            await request.Process();
            await request.ResponseSource.Task;

            _requests.TryRemove(request.ID, out var value);
        }

        /// <summary>
        /// Retrieves a request from the active list, and returns it.
        /// Throws an error if not found
        /// </summary>
        /// <param name="ID">The ID of the request to find</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static IClientRequest RetrieveRequest(string ID)
        {
            if (!_requests.TryGetValue(ID, out var request))
            {
                throw new KeyNotFoundException($"Request with ID {ID} not found");
            }
            return request;
        }

        /// <summary>
        /// Receives the ShutDown signal, preventing new requests from being accepted.
        /// </summary>
        public static void ShutDownSignal()
        {
            lock (ShutdownLock)
            {
                ShutdownFlag = true;

                foreach (IClientRequest request in _requests.Values)
                {
                    try
                    {
                        request.CancellationTokenSource.Cancel();
                    }
                    catch
                    {
                        // Can only fail if request already cancelled.
                    }
                }
            }
        }

        public static async Task ReadyForTermination()
        {   
            // We wait until the dictionary is empty
            while (_requests.Count > 0)
            {
                await Task.Delay(100);
            }
        }
    }
}
