namespace AutomationTestingProgram.Models.Backend
{
    /// <summary>
    /// An interface defining common properties and methods for all requests.
    /// A request is an object representing an api request.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// The unique ID of the request.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// State of the request.
        /// </summary>
        public RequestState State { get; }

        /// <summary>
        /// Message associated with the request. Used to further explain the current state (ex: errors).
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The folder path pointing to a folder containing all logs, files, directories, etc.
        /// </summary>
        public string FolderPath { get; }

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
        void SetStatus(RequestState responseType, string message = "", Exception? e = null);

        /// <summary>
        /// Will validate the request
        /// </summary>
        /// <returns></returns>
        public abstract bool Validate();


        /* REQUEST LIFECYCLE
         * 
         * 1) Received via api request
         * 2) Request Object is created
         * 3) Request is Validated
         * 4) Depending on Request:
         *      - FileValidation Request: Returns validation result
         *      - StopRequest: Sends Stop Request to Request to Stop. Waits for confirmation
         *      - ExecutionRequest: Sent to BrowserManager for processing 
         * 
         */
    }
}
