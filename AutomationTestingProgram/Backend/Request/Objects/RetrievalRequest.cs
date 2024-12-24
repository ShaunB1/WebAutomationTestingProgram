using AutomationTestingProgram.Services.Logging;
using Microsoft.Playwright;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{   
    /// <summary>
    /// Request to retrieve a list of all active requests
    /// </summary>
    public class RetrievalRequest : IClientRequest
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
        public ILogger<RetrievalRequest> Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetrievalRequest"/> class.
        /// </summary>
        public RetrievalRequest() // Can maybe add option to retrieve only a certain # of requests (based on location??)
        {
            ID = Guid.NewGuid().ToString();
            ResponseSource = new TaskCompletionSource();
            State = State.Received;
            StateLock = new object();
            CancellationTokenSource = new CancellationTokenSource();
            Message = string.Empty;
            FolderPath = LogManager.CreateRequestFolder(ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(FolderPath);
            Logger = provider.CreateLogger<RetrievalRequest>()!;
        }

        public void SetStatus(State responseType, string message = "", Exception? e = null)
        {
            lock (StateLock)
            {

                if (ResponseSource!.Task.IsCompleted)
                    return; // DO nothing if already complete

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

                switch (responseType) // Set Exception if Status warrants, but no exception given. Set Result if Completed Status
                {
                    case State.Failure:
                        Logger.LogError($"State: {State}\nMessage: {Message}");
                        ResponseSource!.SetException(new Exception($"{responseType.ToString()} : {message}"));
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
            if (this.CancellationTokenSource.IsCancellationRequested)
            {
                this.SetStatus(State.Cancelled, "Request Cancelled");
                await this.ResponseSource.Task;
            }
        }

        public async Task Process()
        {
            /*
             * Requests can only be completed either in Validate or Execute.
             * If Validate, we no longer perform execute.
             */

            try
            {
                await this.Validate();

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
        /// Validate the <see cref="RetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        private async Task Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application section (requets sent from sections in the application)
             */

            try
            {
                Logger.LogInformation($"Validating Retrieval Request (ID: {ID})");

                await this.IsCancellationRequested();

                // Validate permission to access application
                this.SetStatus(State.Validating, $"Validating User Permissions - Application");

            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Validation Failure", e);
            }
        }

        /// <summary>
        /// Execute the <see cref="RetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        private async Task Execute()
        {
            try
            {
                Logger.LogInformation($"Processing Retrieval Request (ID: {ID})");

                await this.IsCancellationRequested();

                for (int i = 0; i <= 5; i++)
                {
                    // Check if cancellation requested
                    await this.IsCancellationRequested();
                    await Task.Delay(20000);
                    Logger.LogInformation($"{i}");
                }

                this.SetStatus(State.Completed, $"Retrieval Request (ID: {ID}) completed successfully");
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
            if (Logger is CustomLogger<RetrievalRequest> customLogger)
            {
                customLogger.Flush();
            }
        }
    }
}
