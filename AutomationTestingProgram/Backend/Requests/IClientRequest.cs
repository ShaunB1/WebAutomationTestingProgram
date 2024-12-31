using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Interface used by all Request Classes
    /// </summary>
    public interface IClientRequest
    {
        /// <summary>
        /// The unique identifier of the request
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Reference to the User that sent the request
        /// </summary>
        ClaimsPrincipal User { get; }

        /// <summary>
        /// The TaskCompletionSource associated with this request.
        /// When a request is received, the caller await the result of this TaskCompletionSource.
        /// This allows the request to be completed from any point throughout the code.
        /// </summary>
        TaskCompletionSource ResponseSource { get; }

        /// <summary>
        /// The State of the Request.
        /// </summary>
        State State { get; }

        /// <summary>
        /// Lock used to ensure atomic state transitions.
        /// Note: Requests are generally processed single-threaded, except when being cancelled by a CancellationRequest
        /// </summary>
        object StateLock { get; }

        /// <summary>
        /// CancellationTokenSource associated with the request.
        /// Used by CancellationRequest to cancel requests.
        /// </summary>
        [JsonIgnore] // Cannot serialize CancellationTokenSource. Ignore
        CancellationTokenSource CancellationTokenSource { get; }

        /// <summary>
        /// Message associated with the request. Used to further explain the current state (ex: errors)
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The folder path for the request. Folder includes all logs, files, directories, etc.
        /// </summary>
        string FolderPath { get; }


        /// <summary>
        /// Sets the status of the request, including its state and message. 
        /// Optionally, an exception can be provided, which will cause the task to be completed in a failed state.
        /// <para> Depending on the response type (RequestState), the message will be adjusted, and the appropriate result or exception will be set for the `ResponseSource` task. </para>
        /// </summary>
        /// <param name="responseType">
        /// The state of the request, which indicates the outcome of the request processing. 
        /// It can be one of the values from the `RequestState` enumeration (e.g., Completed, Failure, ValidationFailure, etc.).
        /// </param>
        /// <param name="message">
        /// An optional message providing more context about the status. If not provided, the message will default to the `responseType`'s string representation.
        /// </param>
        /// <param name="e">
        /// An optional exception that, if provided, will be appended to the message and set on the `ResponseSource` task, marking it as a failed task.
        /// If no exception is provided, the method will handle success and failure states accordingly.
        /// </param>
        void SetStatus(State responseType, string message = "", Exception? e = null);

        /// <summary>
        /// Checks whether request is flagged for cancellation.
        /// If so, request is cancelled, and OperationCanceledException is thrown.
        /// </summary>
        Task IsCancellationRequested();

        /// <summary>
        /// Process the request.
        /// </summary>
        Task Process();

    }

    /// <summary>
    /// Enum used to describe the current state of a request.
    /// </summary>
    public enum State
    {

        /// <summary>
        /// The request was received. No processing has yet occured.
        /// When a request object is created, this is the State it is initialized with.
        /// </summary>
        Received,

        /// <summary>
        /// The request has been rejected.
        /// This occurs if too many requests are active, or the application is shutting down.
        /// </summary>
        Rejected,

        /// <summary>
        /// The request is queued i.e. -> Waiting before processing.
        /// This usually occurs to prevent too many concurrent requests from processing at once.
        /// There are multiple queues throughout the application. Which one should be specified via message.
        /// </summary>
        Queued,

        /// <summary>
        /// The request is Validating.
        /// This occurs near the begin of a request lifecycle, validating that the request is valid, under various conditions.
        /// There are multiple validations throughout the application. Which one should be specified via message.
        /// </summary>
        Validating,

        /// <summary>
        /// The request is Processing.
        /// This means the request is doing something (not queued, nor validating)
        /// There are many processing areas throughout the application. Which one should be specified via message.
        /// </summary>
        Processing,

        /// <summary>
        /// The request has Failed.
        /// Error message should be provided.
        /// Will return FAILURE.
        /// </summary>
        Failure,

        /// <summary>
        /// The request has Completed.
        /// This occurs if a request has successfully fully completed its task.
        /// Will return SUCCESS.
        /// </summary>
        Completed,

        /// <summary>
        /// The request has been Cancelled.
        /// This occurs if, while the request is in some state doing something, it was cancelled via a CANCELREQUEST.
        /// Will return FAILURE.
        /// </summary>
        Cancelled,
    }
}
