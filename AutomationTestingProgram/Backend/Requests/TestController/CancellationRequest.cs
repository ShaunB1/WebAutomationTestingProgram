using AutomationTestingProgram.Services.Logging;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Request to cancel another request.
    /// NOTE: cannot cancel a CancellationRequest
    /// </summary>
    public class CancellationRequest : IClientRequest
    {
        public string ID { get; }
        [JsonIgnore]
        public ClaimsPrincipal User { get; }
        public State State { get; private set; }
        [JsonIgnore]
        public object StateLock { get; }
        [JsonIgnore] // Cannot serialize. Ignore
        public CancellationTokenSource CancellationTokenSource { get; }
        public string Message { get; private set; }
        public string FolderPath { get; private set; }

        /// <summary>
        /// The Logger object associated with this request
        /// </summary>
        [JsonIgnore]
        public ILogger<CancellationRequest> Logger { get; }

        /// <summary>
        /// The unique identifier of the request to cancel
        /// </summary>
        public string CancelRequestID { get; }

        /// <summary>
        /// The Request to Cancel
        /// </summary>
        public IClientRequest? CancelRequest { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationRequest"/> class.
        /// Instance is associated with the ID of the request to cancel.
        /// </summary>
        /// <param name="ID">The unique identifier of the request to cancel.</param>
        public CancellationRequest(ClaimsPrincipal User, string ID)
        {
            this.ID = Guid.NewGuid().ToString();
            this.User = User;
            CancelRequestID = ID;
            State = State.Received;
            StateLock = new object();
            CancellationTokenSource = new CancellationTokenSource(); // CANNOT CANCEL CANCELLATION REQUEST
            Message = string.Empty;
            FolderPath = LogManager.CreateRequestFolder(this.ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(FolderPath);
            Logger = provider.CreateLogger<CancellationRequest>()!;
        }

        public void SetStatus(State responseType, string message = "", Exception? e = null)
        {
            lock (StateLock)
            {
                if (ResponseSource!.Task.IsCompleted)
                    return; // DO nothing if already complete

                if (responseType == State.Cancelled)
                    throw new Exception("Cannot cancel this Request!");

                State = responseType; // Set the State

                if (string.IsNullOrEmpty(message))
                {
                    message = responseType.ToString();
                }
                Message = message; // Set the Message

                if (e != null) // Set Exception if Exception is Given
                {
                    Message += "\n" + e.ToString();
                    ResponseSource.SetException(e);
                    Logger.LogError($"State: {State}\nMessage: {Message}");
                    return;
                }

                switch (responseType) // If Exception not given
                {
                    case State.Failure:
                        Logger.LogError($"State: {State}\nMessage: {Message}");
                        ResponseSource!.SetException(e);
                        break;
                    case State.Cancelled:
                        Logger.LogError($"State: {State}\nMessage: {Message}");
                        ResponseSource!.SetCanceled();
                        break;
                    case State.Completed:
                        Logger.LogInformation($"State: {State}\nMessage: {Message}");
                        ResponseSource!.SetResult();
                        break;
                    case State.Rejected:
                        Logger.LogInformation($"State: {State}\nMessage: {Message}");
                        this.Flush();
                        break;
                    default:
                        Logger.LogInformation($"State: {State}\nMessage: {Message}");
                        break;
                }
            }
        }

        public async Task IsCancellationRequested()
        {
            // Since CancellationRequests cannot be cancelled, DO NOTHING
            await Task.CompletedTask; 
        }

        public async Task Process()
        {
            try
            {
                this.Validate();

                if (this.ResponseSource!.Task.IsCompleted) // Skip Execute if Validate completed request
                    return;

                await this.Execute();
            }
            catch (Exception e) // Unexpected exception.
            {
                Logger.LogError("Unexpected exception occured.");
                this.SetStatus(State.Failure, "Unexpected exception", e);
            }
            finally
            {
                this.Flush();
            }
            
        }

        /// <summary>
        /// Validate the <see cref="CancellationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        private void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application: Validated with Authorize
             * - User has permission to access application team/group
             * - Request to cancel exists
             * - Request to cancel must not be a Cancellation Request
             * - User has permission to cancel the Request to cancel:
             *      -> If SuperUser: allowed
             *      -> If Admin: Request to Cancel must be within admin's group
             *      -> If User: Request to Cancel must be within user's group, and own request
             */

            try
            {
                this.SetStatus(State.Validating, $"Validating Cancellation Request (ID {this.ID}, CancelID {this.CancelRequestID})");

                // Validate permission to access team
                this.SetStatus(State.Validating, $"Validating User Permissions - Team");

                // Validate Request to Cancel
                this.SetStatus(State.Validating, $"Validating Request to Cancel");
                this.CancelRequest = RequestHandler.RetrieveRequest(this.CancelRequestID);

                this.ValidateRequestType(this.CancelRequest);

                // Validate permissions to cancel request
                this.SetStatus(State.Validating, $"Validating User Permissions - Request to Cancel");

            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Validation Failure", e);
            }

        }

        /// <summary>
        /// Validates whether the Request to Cancel is an appropriate type
        /// </summary>
        /// <param name="request"></param>
        private void ValidateRequestType(IClientRequest request)
        {
            Type type = request.GetType();
            
            switch (type)
            {
                case Type when type == typeof(CancellationRequest):
                    throw new Exception($"Request to cancel (ID: {this.CancelRequestID}) cannot be a CancellationRequest.");

                case Type when type == typeof(KeyChainRetrievalRequest):
                case Type when type == typeof(SecretKeyRetrievalRequest):
                case Type when type == typeof(PasswordResetRequest):
                    throw new Exception($"Request to cancel (ID: {this.CancelRequestID}) cannot be cancelled (invalid type).");
            }
        }

        /// <summary>
        /// Execute the <see cref="CancellationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        private async Task Execute()
        {               
            /*  
             * We set the CancellationTokenSource of the request to Canceled.
             * All requests preriodically check this property while processing/validating.
             * 
             * Issue: What if a request is queued, potentially for a long period of time?
             * - We make state transitions atomic:
             *     -> First make sure request is in queued state
             *     -> Transition it to cancelled state
             *     -> When request finally exists queue, it will remove itself immediatelly
             *     -> If not in queued state, we ignore and leave it up to the request to cancel itself.
             * 
             */


            try
            {
                this.SetStatus(State.Processing, $"Processing Cancellation Request (ID {this.ID}, CancelID {this.CancelRequestID})");

                this.CancelRequest!.CancellationTokenSource.Cancel();
                this.SetStatus(State.Processing, $"Sent Cancellation Request to Request (ID: {this.CancelRequestID})");

                try
                {
                    bool queued = false;
                    lock (this.CancelRequest.StateLock)
                    {
                        if (this.CancelRequest.State == State.Queued)
                        {
                            queued = true;
                        }
                    }

                    if (queued)
                    {
                        await this.CancelRequest.IsCancellationRequested();
                    }

                    await this.CancelRequest!.ResponseSource.Task;
                }
                catch (OperationCanceledException) // Request successfully canceled
                {
                    this.SetStatus(State.Completed, $"Request (ID: {this.CancelRequestID}) cancelled successfully");
                }
                catch (Exception e)
                {
                    // Request failed before cancellation received/processed
                    throw new Exception($"Request (ID: {this.CancelRequestID}) failed before cancellation received/processed");
                }

                // Request completed before cancellation received/processed
                throw new Exception($"Request (ID: {this.CancelRequestID}) completed before cancellation received/processed");

            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Processing Failure", e);
            }
        }

        /// <summary>
        /// Flush all logs.
        /// </summary>
        private void Flush()
        {
            if (Logger is CustomLogger<CancellationRequest> customLogger)
            {
                customLogger.Flush();
            }
        }
    }
}
