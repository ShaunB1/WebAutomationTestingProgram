using System.Security.Claims;
using System.Text.Json.Serialization;
using AutomationTestingProgram.Core.Services.Logging;

namespace AutomationTestingProgram.Core.Helpers.Requests
{
    /// <summary>
    /// Abstract class defining all Non-Cancellable Request Types
    /// </summary>
    public abstract class NonCancellableClientRequest : IClientRequest
    {
        public string Id { get; }
        [JsonIgnore]
        public ClaimsPrincipal User { get; }
        [JsonIgnore]
        public TaskCompletionSource ResponseSource { get; }
        public State State { get; private set; }
        public string Message { get; private set; }
        [JsonIgnore]
        public string StackTrace { get; private set; }
        public string FolderPath { get; }

        /// <summary>
        /// The Logger object associated with this request
        /// </summary>
        [JsonIgnore]
        protected abstract ICustomLogger? Logger { get; }

        /// <summary>
        /// Lock used to prevent concurrency issues during state transitions
        /// </summary>
        [JsonIgnore]
        private readonly object _stateLock;

        /// <summary>
        /// Initializes variables to be used for all Cancellable Request Types
        /// </summary>
        protected NonCancellableClientRequest(ClaimsPrincipal user, bool isLoggingEnabled = true)
        {
            Id = Guid.NewGuid().ToString();
            User = user;
            ResponseSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            State = State.Received;
            Message = string.Empty;
            StackTrace = string.Empty;
            _stateLock = new object();
            FolderPath = isLoggingEnabled ? LogManager.CreateRequestFolder(Id) : string.Empty;
        }

        public void SetStatus(State responseType, string message = "", Exception? e = null)
        {
            lock (_stateLock)
            {
                if (State == State.Cancelled)
                    throw new InvalidOperationException($"Cannot cancel {GetType().Name} type");

                if (ResponseSource.Task.IsCompleted)
                    return; // Do nothing if already complete.

                State = responseType; // Set the State

                if (string.IsNullOrEmpty(message))
                {
                    message = responseType.ToString();
                }

                Message = message; // Set the Message

                switch (State)
                {
                    case State.Failure:
                        if (e != null) // If exception given, add to message
                        {
                            Message += ": " + e.Message;
                            StackTrace += e.StackTrace;
                            ResponseSource.SetException(e);
                        }
                        else
                        {
                            ResponseSource.SetException(new Exception($"{responseType.ToString()} : {message}"));
                        }
                        LogError($"State: {State}\nMessage: {Message}\nStack Trace: {StackTrace}");
                        break;
                    case State.Rejected:
                        ResponseSource.SetCanceled();
                        LogInfo($"State: {State}\nMessage: {Message}");
                        Flush();
                        break;
                    case State.Completed:
                        ResponseSource.SetResult();
                        LogInfo($"State: {State}\nMessage: {Message}");
                        break;
                    default:
                        LogInfo($"State: {State}\nMessage: {Message}");
                        break;
                }
            }
        }

        public async Task Process()
        {
            try
            {
                Validate();

                await Execute();
            }
            catch (OperationCanceledException)
            {
                SetStatus(State.Cancelled, "Cancelled");
            }
            catch (Exception e)
            {
                SetStatus(State.Failure, $"Failure (State: {State})", e);
            }
            finally
            {
                Flush();
            }
        }

        /// <summary>
        /// Validate the request.
        /// View inner documentation of implementing class on specifics.  
        /// </summary>
        protected abstract void Validate();

        /// <summary>
        /// Execute the request.
        /// View inner documentation of implementing class on specifics.
        /// </summary>
        /// <returns></returns>
        protected abstract Task Execute();

        public void LogInfo(string message)
        {
            Logger?.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            Logger?.LogWarning(message);
        }

        public void LogError(string message)
        {
            Logger?.LogError(message);
        }

        public void LogCritical(string message)
        {
            Logger?.LogCritical(message);
        }

        public Task Log(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    LogCritical(message); break;
                case LogLevel.Error:
                    LogError(message); break;
                case LogLevel.Warning:
                    LogWarning(message); break;
                case LogLevel.Information:
                    LogInfo(message); break;
                default:
                    throw new NotImplementedException($"Log level not implemented: {level.ToString()}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Flush all logs
        /// </summary>
        private void Flush()
        {
            Logger?.Flush();
        }
    }
}
