using System.Security.Claims;
using System.Text.Json.Serialization;
using AutomationTestingProgram.Core.Helpers.Requests;
using AutomationTestingProgram.Core.Models.Requests;
using AutomationTestingProgram.Core.Services;
using AutomationTestingProgram.Core.Services.Logging;

namespace AutomationTestingProgram.Core.Requests
{
    /// <summary>
    /// Request to cancel another request.
    /// NOTE: cannot cancel a CancellationRequest
    /// </summary>
    public class CancellationRequest : NonCancellableClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// The unique identifier of the request to cancel
        /// </summary>
        public string CancelRequestId { get; }

        /// <summary>
        /// The Request to Cancel
        /// </summary>
        [JsonIgnore] // Will not serialize for security reasons. Can update to serialize only after validations pass with custom serialization logic
        private CancellableClientRequest? CancelRequest { get; set; }

        [JsonIgnore]
        private RequestHandler _requestHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationRequest"/> class.
        /// Instance is associated with the ID of the request to cancel.
        /// </summary>
        /// <param name="ID">The unique identifier of the request to cancel.</param>
        public CancellationRequest(ICustomLoggerProvider provider, RequestHandler requestHandler, ClaimsPrincipal User, CancellationRequestModel model)
            : base(User)
        {
            Logger = provider.CreateLogger<CancellationRequest>(FolderPath);
            CancelRequestId = model.ID;
            _requestHandler = requestHandler;
        }

        /// <summary>
        /// Validate the <see cref="CancellationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application: Validated with Authorize
             * - User has permission to access application team/group
             * - Request to cancel exists
             * - Request to cancel must not be Cancellable
             * - User has permission to cancel the Request to cancel:
             *      -> If SuperUser: allowed
             *      -> If Admin: Request to Cancel must be within admin's group
             *      -> If User: Request to Cancel must be within user's group, and own request
             */

            SetStatus(State.Validating, $"Validating Cancellation Request (ID {Id}, CancelID {CancelRequestId})");

            // Validate permission to access team
            LogInfo($"Validating User Permissions - Team");

            // Validate Request to Cancel
            LogInfo($"Validating Request to Cancel");
            ValidateRequest();

            // Validate permissions to cancel request
            LogInfo($"Validating User Permissions - Request to Cancel");
        }

        /// <summary>
        /// Validates the Request to Cancel
        /// </summary>
        private void ValidateRequest()
        {
            IClientRequest request = _requestHandler.RetrieveRequest(CancelRequestId);

            if (request is NonCancellableClientRequest)
            {
                throw new Exception($"Request to cancel (ID: {CancelRequestId}) cannot be cancelled (invalid type).");
            }
            else
            {
                CancelRequest = (CancellableClientRequest)request;
            }
        }

        /// <summary>
        /// Execute the <see cref="CancellationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            /*  
             * All cancellable requests preriodically check their CancellationTokenSource.
             * 
             * Issue: What if a request is queued, potentially for a long period of time?
             * - We make sure all waits/queues use semaphores (in some capacity)
             * - CancellationTokens can be added to semaphores. This will allow
             *   the request to immediatelly cancel itself even if waiting in a queue
             *   for long periods of time (no processing)
             * 
             */
            SetStatus(State.Processing, $"Processing Cancellation Request (ID {Id}, CancelID {CancelRequestId})");
            CancelRequest!.Cancel();
            LogInfo($"Sent Cancellation Request to Request (ID: {CancelRequestId})");

            try
            {
                await CancelRequest!.ResponseSource.Task;

                // Request completed before cancellation received/processed
                throw new Exception($"Request (ID: {CancelRequestId}) completed before cancellation received/processed");
            }
            catch (OperationCanceledException) // Request successfully canceled
            {
                SetStatus(State.Completed, $"Request (ID: {CancelRequestId}) cancelled successfully");
            }
            catch (Exception)
            {
                // Request failed before cancellation received/processed
                throw new Exception($"Request (ID: {CancelRequestId}) failed before cancellation received/processed");
            }
        }
    }
}
