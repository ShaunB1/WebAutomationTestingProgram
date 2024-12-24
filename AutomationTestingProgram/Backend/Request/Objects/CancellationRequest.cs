using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Playwright;
using Org.BouncyCastle.Asn1.Ocsp;
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
        public TaskCompletionSource ResponseSource { get; }
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
        public CancellationRequest(string ID)
        {
            this.ID = Guid.NewGuid().ToString();
            CancelRequestID = ID;
            ResponseSource = new TaskCompletionSource();
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
                    throw new Exception("Cannot cancel a Cancellation Request!");

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
                    default:
                        Logger.LogInformation($"State: {State}\nMessage: {Message}");
                        break;
                }
            }
        }
        public string GetStatus()
        {
            lock (StateLock)
            {
                if (string.IsNullOrEmpty(Message))
                {
                    return $"ID: {ID} | State: {State.ToString()}";
                }
                else
                {
                    return $"ID: {ID} | State: {State.ToString()} | Message: {Message}";
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
            /*
             * Requests can only be completed either in Validate or Execute.
             * If Validate, we no longer perform execute.
             */

            try
            {
                this.Validate();

                if (this.ResponseSource!.Task.IsCompleted)
                    return;

                await this.Execute();
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
             * - User has permission to access application
             * - User has permission to access application section (requests sent from sections in the application)'
             * - Request to cancel exists
             * - Request to cancel must not be a Cancellation Request
             * - User has permission to cancel the Request to cancel:
             *      -> If SuperUser: allowed
             *      -> If Admin: Request to Cancel must be within admin's section
             *      -> If User: Request to Cancel must be within user's section, and own request
             */

            try
            {
                this.SetStatus(State.Validating, $"Validating Cancellation Request (ID {this.ID}, CancelID {this.CancelRequestID})");

                // Validate permission to access application
                this.SetStatus(State.Validating, $"Validating User Permissions - Application");

                // Validate Request to Cancel
                this.SetStatus(State.Validating, $"Validating Request to Cancel");
                this.CancelRequest = RequestHandler.RetrieveRequest(this.CancelRequestID);

                if (this.CancelRequest is CancellationRequest)
                    throw new Exception($"Request to cancel (ID: {this.CancelRequestID}) cannot be a CancellationRequest.");

                // Validate permissions to cancel request
                this.SetStatus(State.Validating, $"Validating User Permissions - Request to Cancel");

            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Validation Failure", e);
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
                    this.SetStatus(State.Completed, $"Cancellation Request (ID: {this.ID}) completed successfully. Request (ID: {this.CancelRequestID}) cancelled successfully");
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
