using AutomationTestingProgram.Services.Logging;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend.Request
{
    /// <summary>
    /// Request to shut down the whole application.
    /// NOTE: ShutDown is irreversible. Only one may process, rest are ignored.
    /// </summary>
    public class ShutDownRequest : IRequest<ShutDownRequest>
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
        public ILogger<ShutDownRequest> Logger { get; }

        /// <summary>
        /// Whether the ShutDown is Graceful or Forceful
        /// </summary>
        public bool Force { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShutDownRequest"/> class.
        /// </summary>
        /// <param name="Force">Whether the ShutDown is Graceful or Forceful</param>
        public ShutDownRequest(bool Force)
        {
            this.ID = Guid.NewGuid().ToString();
            this.Force = Force;
            ResponseSource = new TaskCompletionSource();
            State = State.Received;
            StateLock = new object();
            CancellationTokenSource = null; // CANNOT CANCEL SHUTDOWN REQUEST
            Message = string.Empty;
            FolderPath = LogManager.CreateRequestFolder(this.ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(FolderPath);
            Logger = provider.CreateLogger<ShutDownRequest>()!;
        }

        public void SetStatus(State responseType, string message = "", Exception? e = null)
        {
            lock (StateLock)
            {
                if (ResponseSource!.Task.IsCompleted)
                    return; // DO nothing if already complete

                if (responseType == State.Cancelled)
                    throw new Exception("Cannot cancel a ShutDown Request!");

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

        public async Task<ShutDownRequest> Execute()
        {
            /* If Forceful -> Will just close whole application immediatelly. Flushes all logs.
             * If Graceful:
             * - Sends Cancellation Flag to all non-Cancellation requests.
             * - Waits for ActiveRequests Dictionary to be empty
             */

            Logger.LogInformation($"ShutDown Request (ID: {ID}) received.");
            if (Force)
            {
                // Forceful shutdown: Immediatelly close the application
                Logger.LogInformation($"Forceful shutdown initiated.");
                if (Logger is CustomLogger<ShutDownRequest> customLogger)
                {
                    customLogger.FlushAll();
                }
                Environment.Exit(0);
            }
            else
            {
                // Graceful shutdown: Sends cancellation requests, and makes sure application ends in a stable state
                Logger.LogInformation($"Graceful shutdown initiated.");
                /*
                 * Sends cancellation requests to all non-CancellationRequests.
                 * Wait for ActiveDictionary to be empty.
                 * FlushAll
                 * Environment.Exit(0)
                 * Needs cancellationrequest to be completed first
                 */
            }

            return this;
        }
    }
}
