using System.Collections.Concurrent;
using AutomationTestingProgram.Core.Helpers.Requests;
using AutomationTestingProgram.Core.Settings.Request;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Core.Services
{
    /// <summary>
    /// The Request Handler handles all incoming requests.
    /// </summary>
    public class RequestHandler
    {
        /// <summary>
        /// Settings used for Requests
        /// </summary>
        private readonly RequestSettings _settings;

        /// <summary>
        /// Semaphore used to limit total # of active requests
        /// </summary>
        private readonly SemaphoreSlim _maxRequests;

        /// <summary>
        /// Dictionary of all active requests, keyed by their id
        /// </summary>
        private readonly ConcurrentDictionary<string, IClientRequest> _requests;

        /// <summary>
        /// Token detecting whether the application is shutting down. Used to prevent new requests
        /// from being received.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        public RequestHandler(IOptions<RequestSettings> options)
        {
            _settings = options.Value;
            _maxRequests = new SemaphoreSlim(_settings.Limit);
            _requests = new ConcurrentDictionary<string, IClientRequest>();
            _tokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Processes a given request, and returns its result
        /// </summary>
        /// <param name="request">The request to process. Can either be CancellableClientRequest or NonCancellableClientRequest</param>
        /// <returns>The result of the request</returns>
        public async Task ProcessAsync(IClientRequest request)
        {
            try
            {
                _requests.TryAdd(request.Id, request);

                request.SetStatus(State.Received, $"{request.GetType().Name} (ID: {request.Id}) received.");

                await request.Process();
                await request.ResponseSource.Task;
                // If an exception, caught by controller
            }
            finally
            {
                _requests.TryRemove(request.Id, out var value);
            }
        }

        /// <summary>
        /// Tries to access a slot for a request to be processed
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TryAcquireSlotAsync(int timeout = 30000) // 30 seconds
        {
            if (!_tokenSource.IsCancellationRequested)
                return await _maxRequests.WaitAsync(timeout);

            return false;
        }

        /// <summary>
        /// Releases a slot for other requests to start processing
        /// </summary>
        public void ReleaseSlot()
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
        public IClientRequest RetrieveRequest(string ID)
        {
            if (!_requests.TryGetValue(ID, out var request))
            {
                throw new KeyNotFoundException($"Request with ID {ID} not found");
            }
            return request;
        }

        /// <summary>
        /// Retrieves a list of all requests from the active list, filters it, and returns it.
        /// </summary>
        /// <param name="filter">The filter used</param>
        /// <returns></returns>
        public IList<IClientRequest> RetrieveRequests(string filter = "")
        {
            if (string.IsNullOrEmpty(filter))
            {
                return _requests.Values.ToList();
            }
            
            Type? filterType = Type.GetType(filter);

            if (filterType == null)
            {
                throw new ArgumentException($"Invalid filter type: {filter}");
            }

            IList<IClientRequest> requests = new List<IClientRequest>();

            foreach (var value in _requests.Values)
            {
                if (filterType.IsAssignableFrom(value.GetType()))
                {
                    requests.Add(value);
                }
            }

            return requests;
        }

        /// <summary>
        /// Receives the ShutDown signal, preventing new requests from being accepted.
        /// </summary>
        public async Task ShutDownAsync()
        {
            _tokenSource.Cancel(); // Cancels the token source

            // Waits for all requests to be in the dictionary (concurrency issues)
            while (_maxRequests.CurrentCount != _settings.Limit - _requests.Count)
            {
                await Task.Delay(100);
            }

            // Cancels all requests in dictionary (if possible)
            foreach (IClientRequest request in _requests.Values)
            {
                try
                {
                    if (request is CancellableClientRequest cancellableClientRequest)
                    {
                        cancellableClientRequest.Cancel();
                    }
                }
                catch
                {
                    // Can only fail if request already cancelled.
                }
            }

            // Waits for semaphore to the full
            while (_maxRequests.CurrentCount != _settings.Limit)
            {
                await Task.Delay(100);
            }
        }
    }
}
