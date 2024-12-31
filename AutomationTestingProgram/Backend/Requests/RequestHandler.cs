using AutomationTestingProgram.Backend.Helpers;
using AutomationTestingProgram.Models.Settings;
using Microsoft.Graph.Models;
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
        public static readonly PlaywrightObject _playwright;

        /// <summary>
        /// Settings used for Requests
        /// </summary>
        public static readonly RequestSettings _requestSettings;

        /// <summary>
        /// Semaphore used to limit total # of active requests
        /// </summary>
        private static readonly SemaphoreSlim _maxRequests;

        /// <summary>
        /// Dictionary of all active requests, keyed by their id
        /// </summary>
        private static readonly ConcurrentDictionary<string, IClientRequest> _requests;

        /// <summary>
        /// Token detecting whether the application is shutting down. Used to prevent new requests
        /// from being received.
        /// </summary>
        private static CancellationTokenSource _tokenSource;

        static RequestHandler()
        {
            _playwright = new PlaywrightObject();
            _requestSettings = AppConfiguration.GetSection<RequestSettings>("Request");
            _maxRequests = new SemaphoreSlim(_requestSettings.Limit);
            _requests = new ConcurrentDictionary<string, IClientRequest>();
            _tokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Processes a given request, and returns its result
        /// </summary>
        /// <param name="request">The request to process</param>
        /// <returns>The result of the request</returns>
        public static async Task ProcessRequestAsync(IClientRequest request)
        {
            _requests.TryAdd(request.ID, request);

            request.SetStatus(State.Received, $"{request.GetType().Name} (ID: {request.ID}) received.");

            await request.Process();
            await request.ResponseSource.Task;
            // If an exception, caught by controller

            _requests.TryRemove(request.ID, out var value);
        }

        /// <summary>
        /// Tries to access a slot for a request to be processed
        /// </summary>
        /// <returns></returns>
        public static bool TryAcquireRequestSlot()
        {
            if (!_tokenSource.IsCancellationRequested && _maxRequests.Wait(0))
                return true;

            return false;
        }

        /// <summary>
        /// Releases a slot for other requests to start processing
        /// </summary>
        public static void ReleaseRequestSlot()
        {
            _maxRequests.Release();
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
        public static async Task ShutDownAsync()
        {
            _tokenSource.Cancel(); // Cancels the token source

            // Waits for all requests to be in the dictionary (concurrency issues)
            while (_maxRequests.CurrentCount != _requests.Count)
            {
                await Task.Delay(100);
            }

            // Cancels all requests in dictionary (if possible)
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

            // Waits for semaphore to the full
            while (_maxRequests.CurrentCount != _requestSettings.Limit)
            {
                await Task.Delay(100);
            }
        }
    }
}
