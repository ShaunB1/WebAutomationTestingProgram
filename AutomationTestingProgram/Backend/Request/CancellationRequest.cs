using AutomationTestingProgram.ModelsOLD;
using AutomationTestingProgram.Services.Logging;
using Microsoft.Playwright;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Request to cancel another request.
    /// NOTE: cannot cancel a CancellationRequest
    /// </summary>
    public class CancellationRequest : IRequest<CancellationRequest>
    {
        public string ID { get; }
        [JsonIgnore]
        public TaskCompletionSource ResponseSource { get; }
        public State State { get; private set; }
        [JsonIgnore]
        public object StateLock { get; }
        [JsonIgnore] // Cannot serialize. Ignore
        public CancellationTokenSource? CancellationTokenSource { get; }
        public string Message { get; private set; }
        public string FolderPath { get; private set; }
        [JsonIgnore]
        public ILogger<CancellationRequest> Logger { get; }

        /// <summary>
        /// The unique identifier of the request to cancel
        /// </summary>
        public string CancelRequestID { get; }

        /// <summary>
        /// The Request to Cancel
        /// </summary>
        public IRequest<object>? CancelRequest { get; private set; }


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
            CancellationTokenSource = null; // CANNOT CANCEL CANCELLATION REQUEST
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
                    return;
                }

                switch (responseType) // Set Exception if Status warrants, but no exception given. Set Result if Completed Status
                {
                    case State.Failure:
                        ResponseSource!.SetException(new Exception($"{responseType.ToString()} : {message}"));
                        break;
                    case State.Completed:
                        ResponseSource!.SetResult();
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

        public void SetPath(string folderPath)
        {
            FolderPath = folderPath;
        }

        public async Task<CancellationRequest> Execute()
        {
            /* IDEA: When canceling a request:
             * - Request must exist
             * - Request must not be a Cancellation Request
             * 
             * We set the CancellationFlag to true.
             * Requests check this flag throughout the code to cancel themselves.
             * 
             * Issue: What if a request is queued for a long period of time?
             * -> We make state transitions atomic
             * -> Once entered lock, we make sure its in a queued state -> then cancelled
             * -> If not in a queued state, we ignore, and leave it up to the request to cancel itself.
             * 
             * 
             */

            Logger.LogInformation($"Cancellation Request (ID: {ID}, RequestCancelID: {CancelRequestID}) received.");
            await Task.Delay(20000);
            ResponseSource.SetResult();

            try
            {
                await ResponseSource.Task;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while awaiting cancellation request: {e.Message}.");
            }

            switch (ResponseSource.Task.Status)
            {
                case TaskStatus.RanToCompletion:
                    // Task completed successfully -> SetResult
                    Logger.LogInformation($"Cancellation Request (ID: {ID}) COMPLETED successfully.");
                    break;

                case TaskStatus.Faulted:
                    // Task failed -> SetException
                    var exceptionMessage = ResponseSource.Task.Exception?.ToString() ?? "Unknown error.";
                    Logger.LogError($"Cancellation Request (ID: {ID}) FAILED:\n{exceptionMessage}");
                    break;

                case TaskStatus.Canceled:
                    // Task was canceled -> SetCanceled
                    Logger.LogError($"Cancellation Request (ID: {ID}) was CANCELED!! Investigate. CancellationRequests should not be cancellable");
                    break;
            }


            if (Logger is CustomLogger<CancellationRequest> customLogger)
            {
                customLogger.Flush();
            }

            return this;

        }

        /// <summary>
        /// Link the found request to this Cancellation Request
        /// </summary>
        /// <param name="request"></param>
        public void LinkRequest(IRequest<object> request)
        {
            CancelRequest = request;
        }
    }
}
