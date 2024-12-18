using AutomationTestingProgram.Services.Logging;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Request to process a test file using playwright
    /// </summary>
    public class ProcessRequest : IClientRequest
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
        public ILogger<ProcessRequest> Logger { get; }


        /// <summary>
        /// The file provided with the request
        /// </summary>
        public IFormFile File { get; }

        /// <summary>
        /// The browser TYPE used to process the request
        /// </summary>
        public string BrowserType { get; }

        /// <summary>
        /// The browser VERSION used to process the request
        /// </summary>
        public string BrowserVersion { get; }

        /// <summary>
        /// The environment used to process the request
        /// </summary>
        public string Environment { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRequest"/> class.
        /// Instance is associated with a file and the specified browser type and version.
        /// </summary>
        /// <param name="File">The file to be processed in the request.</param>
        /// <param name="Type">The type of the browser (e.g., "Chrome", "Firefox") that will handle the request.</param>
        /// <param name="Version">The version of the browser (e.g., "91", "93") that will be used to process the request.</param>
        public ProcessRequest(IFormFile File, string Type, string Version, string Environment)
        {
            ID = Guid.NewGuid().ToString();
            this.File = File;
            BrowserType = Type;
            BrowserVersion = Version;
            this.Environment = Environment;
            ResponseSource = new TaskCompletionSource();
            State = State.Received;
            StateLock = new object();
            CancellationTokenSource = new CancellationTokenSource();
            Message = string.Empty;
            FolderPath = LogManager.CreateRequestFolder(ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(FolderPath);
            Logger = provider.CreateLogger<ProcessRequest>()!;
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
                        ResponseSource!.SetException(new Exception($"{responseType.ToString()} : {message}"));
                        break;
                    case State.Cancelled:
                        ResponseSource!.SetCanceled();
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
            Logger.LogInformation($"Process Request (ID: {ID}, BrowserType: {BrowserType}," +
                $" BrowserVersion: {BrowserVersion}, Environment: {Environment}) received.");
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
                Logger.LogError($"Error while awaiting process request: {e.Message}.");
            }

            switch (ResponseSource.Task.Status)
            {
                case TaskStatus.RanToCompletion:
                    // Task completed successfully -> SetResult
                    Logger.LogInformation($"Process Request (ID: {ID}, BrowserType: {BrowserType}, " +
                        $"BrowserVersion: {BrowserVersion}, Environment: {Environment}) COMPLETED successfully.");
                    break;

                case TaskStatus.Faulted:
                    // Task failed -> SetException
                    var exceptionMessage = ResponseSource.Task.Exception?.ToString() ?? "Unknown error.";
                    Logger.LogError($"Process Request (ID: {ID}, BrowserType: {BrowserType}, " +
                        $"BrowserVersion: {BrowserVersion}, Environment: {Environment}) FAILED:\n{exceptionMessage}.");
                    break;

                case TaskStatus.Canceled:
                    // Task was canceled -> SetCanceled
                    Logger.LogError($"Process Request (ID: {ID}, BrowserType: {BrowserType}, " +
                        $"BrowserVersion: {BrowserVersion}, Environment: {Environment}) CANCELED.");
                    break;
            }

            if (Logger is CustomLogger<ProcessRequest> customLogger)
            {
                customLogger.Flush();
            }
        }
    }
}
