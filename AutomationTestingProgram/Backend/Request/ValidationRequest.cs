using AutomationTestingProgram.Services.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Request to validate a test file.
    /// </summary>
    public class ValidationRequest : IClientRequest
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
        public ILogger<ValidationRequest> Logger { get; }


        /// <summary>
        /// The file provided with the request
        /// </summary>
        public IFormFile File { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRequest"/> class.
        /// Instance is associated with a file.
        /// </summary>
        /// <param name="File">The file to be validated in the request.</param>
        public ValidationRequest(IFormFile File)
        {
            ID = Guid.NewGuid().ToString();
            this.File = File;
            ResponseSource = new TaskCompletionSource();
            State = State.Received;
            StateLock = new object();
            CancellationTokenSource = new CancellationTokenSource();
            Message = string.Empty;
            FolderPath = LogManager.CreateRequestFolder(ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(FolderPath);
            Logger = provider.CreateLogger<ValidationRequest>()!;
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
                    return;
                }

                switch (responseType) // Set Exception if Status warrants, but no exception given. Set Result if Completed Status
                {
                    case State.Failure:
                    case State.Cancelled:
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

        public async Task Execute()
        {
            Logger.LogInformation($"Validation Request (ID: {ID}) received.");
            await Task.Delay(20000);
            Logger.LogInformation($"First");
            await Task.Delay(20000);
            Logger.LogInformation($"Second");
            await Task.Delay(20000);
            Logger.LogInformation($"Third");
            await Task.Delay(20000);
            Logger.LogInformation($"Fourth");
            await Task.Delay(20000);
            Logger.LogInformation($"Fifth");
            await Task.Delay(20000);
            ResponseSource.SetResult();

            try
            {
                await ResponseSource.Task;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while awaiting validation request: {e.Message}.");
            }

            switch (ResponseSource.Task.Status)
            {
                case TaskStatus.RanToCompletion:
                    // Task completed successfully -> SetResult
                    Logger.LogInformation($"Process Request (ID: {ID}) COMPLETED successfully.");
                    break;

                case TaskStatus.Faulted:
                    // Task failed -> SetException
                    var exceptionMessage = ResponseSource.Task.Exception?.ToString() ?? "Unknown error.";
                    Logger.LogError($"Process Request (ID: {ID}) FAILED:\n{exceptionMessage}.");
                    break;

                case TaskStatus.Canceled:
                    // Task was canceled -> SetCanceled
                    Logger.LogError($"Process Request (ID: {ID})) CANCELED.");
                    break;
            }


            if (Logger is CustomLogger<ValidationRequest> customLogger)
            {
                customLogger.Flush();
            }
        }
    }
}
