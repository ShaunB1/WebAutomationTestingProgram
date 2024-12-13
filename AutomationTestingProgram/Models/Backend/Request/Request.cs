namespace AutomationTestingProgram.Models.Backend
{   
    /// <summary>
    /// Represents an api request sent for processing.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// The unique ID of the request.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The file provided with the request.
        /// </summary>
        public IFormFile File { get; }

        /// <summary>
        /// The browser type used to process the request.
        /// </summary>
        public string BrowserType { get; }

        /// <summary>
        /// The browser version used to process the request.
        /// </summary>
        public int BrowserVersion { get; }

        /// <summary>
        /// The task completion source associated with this request.
        /// </summary>
        public TaskCompletionSource<Request>? ResponseSource { get; }

        /// <summary>
        /// The state of the request.
        /// </summary>
        public RequestState State { get; private set; }

        /// <summary>
        /// Message associated with the request. Used to further explain the current state (ex: errors).
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The context (test run) level folder path with all logs, files, directories.
        /// </summary>
        public string FolderPath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// Instance is associated with a file and the specified browser type and version.
        /// </summary>
        /// <param name="File">The file to be processed in the request.</param>
        /// <param name="Type">The type of the browser (e.g., "Chrome", "Firefox") that will handle the request.</param>
        /// <param name="Version">The version of the browser (e.g., "91", "93") that will be used to process the request.</param>
        public Request(IFormFile File, string Type, int Version)
        {
            this.ID = Guid.NewGuid().ToString();
            this.File = File;
            this.BrowserType = Type;
            this.BrowserVersion = Version;
            this.ResponseSource = new TaskCompletionSource<Request>();
            this.State = RequestState.NotYetProcessed;
            this.Message = string.Empty;
            this.FolderPath = string.Empty;
        }

        /// <summary>
        /// Sets the status of the request, including its state and message. 
        /// Optionally, an exception can be provided, which will cause the task to be completed in a failed state.
        /// Depending on the response type (RequestState), the message will be adjusted, and the appropriate result or exception will be set for the `ResponseSource` task.
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
        public void SetStatus(RequestState responseType, string message = "", Exception? e = null)
        {
            if (this.ResponseSource!.Task.IsCompleted)
                return;
            
            this.State = responseType;
            if (string.IsNullOrEmpty(message))
            {
                message = responseType.ToString();
            }

            this.Message = message;

            if (e != null)
            {
                this.Message += e.ToString();
                this.ResponseSource!.SetException(e);
                return;
            }

            switch (responseType)
            {
                case RequestState.ValidationFailure:
                case RequestState.ProcessingFailure:
                case RequestState.Failure:
                case RequestState.Cancelled:
                    this.ResponseSource!.SetException(new Exception($"{responseType.ToString()} : {message}"));
                    break;
                case RequestState.Completed:
                    this.ResponseSource!.SetResult(this);
                    break;
            }
        }

        /// <summary>
        /// Retrieve a string to state the current status of the request.
        /// </summary>
        /// <returns>A string message</returns>
        public string GetStatus()
        {
            if (string.IsNullOrEmpty(this.Message))
            {
                return $"ID: {ID} | Result: {State.ToString()}";
            }
            else
            {
                return $"ID: {ID} | Result: {State.ToString()} | Message: {Message}";
            }
        }

        /// <summary>
        /// Sets the folder path of the request
        /// </summary>
        /// <param name="folderPath">The folderpath to set</param>
        public void SetPath(string folderPath)
        {
            this.FolderPath = folderPath;
        }

        /// <summary>
        /// Stops processing for the given request
        /// </summary>
        /// <returns></returns>
        public void CancelRequest()
        {
            /* When a request is cancelled, the following MUST be done.
             * 
             * 1. Set state to cancelled. This will cancel the request.
             * 2. If request is queued -> FOR NOW, remains in queue. Once started, we check the state, and stop.
             * 3. If request not queued -> Was processing somewhere. Add checks for states throughout the process to check for 
             * cancellation and handle it gracefully.
             */
            
            SetStatus(RequestState.Cancelled, "Stop request sent - Cancelled");

            // Better idea, make a lock. While request is doing something that can't be cancelled. Lock. 
            // Every time I request the lock, it will then be able to be cancelled

        }
    }


    public enum RequestState
    {

        /// <summary>
        /// Not Yet Processed
        /// </summary>
        NotYetProcessed,

        /// <summary>
        /// Queued
        /// </summary>
        Queued,

        /// <summary>
        /// Validating
        /// </summary>
        Validating,

        /// <summary>
        /// Failed Validation
        /// </summary>
        ValidationFailure,

        /// <summary>
        /// Processing
        /// </summary>
        Processing,

        /// <summary>
        /// Failed Processing
        /// </summary>
        ProcessingFailure,

        /// <summary>
        /// Completed
        /// </summary>
        Completed,

        /// <summary>
        /// Failure
        /// </summary>
        Failure,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled
    }
}
