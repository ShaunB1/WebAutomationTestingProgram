using DocumentFormat.OpenXml.Office2010.Excel;
using System.Collections.Concurrent;

namespace AutomationTestingProgram.Backend
{   
    /// <summary>
    /// The Request Handler handles all incoming requests, including validation and processing.
    /// </summary>
    public static class RequestHandler
    {   
        /// <summary>
        /// Dictionary of all active requests, keyed by their id
        /// </summary>
        private static readonly ConcurrentDictionary<string, IClientRequest> _requests;

        /// <summary>
        /// Limits total # of received requests by the framework
        /// </summary>
        private static readonly SemaphoreSlim _maxRequests;

        /// <summary>
        /// Limits total # of opened files in memory APPLICATION WIDE
        /// </summary>
        private static readonly SemaphoreSlim _maxOpenFiles;

        /// <summary>
        /// Flag detecting whether the application is shutting down. Used to prevent new requests
        /// from being received.
        /// </summary>
        private static bool ShutdownFlag = false;
        private static readonly object ShutdownLock = new object();

        static RequestHandler()
        {   
            _requests = new ConcurrentDictionary<string, IClientRequest>();
            _maxRequests = new SemaphoreSlim(150);
            _maxOpenFiles = new SemaphoreSlim(50);
        }

        public static async Task ProcessRequestAsync(IClientRequest request)
        {
            request.SetStatus(State.Received, $"{request.GetType().Name} (ID: {request.ID}) received.");

            await request.Process();
        }

        /// <summary>
        /// Tries to aquire a slot for a request to be accepted by the application.
        /// </summary>
        /// <returns>Whether the request successfully aquired a slot</returns>
        public static bool TryAcquireRequestSlot(IClientRequest request)
        {
            lock (ShutdownLock)
            {
                if (ShutdownFlag)
                {
                    return false;
                }

                // Adding to dictionary inside lock for concurrency reasons
                if (_maxRequests.Wait(0))
                {
                    _requests.TryAdd(request.ID, request);
                    return true;
                }
            }            

            return false;
        }

        /// <summary>
        /// Releases a slot for new requests to be accepted by the application
        /// </summary>
        public static void ReleaseRequestSlot(IClientRequest request)
        {
            _requests.TryRemove(request.ID, out var value);
            _maxRequests.Release();
        }

        /// <summary>
        /// Waits for a slot to be allowed to open a file. 
        /// </summary>
        public static async Task TryAquireFileSlotAsync()
        {
            await _maxOpenFiles.WaitAsync();
        }

        /// <summary>
        /// Releases a slot for new files to be opened by the application
        /// </summary>
        public static void ReleaseFileSlot()
        {
            _maxOpenFiles.Release();
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
