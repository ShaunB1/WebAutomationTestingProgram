using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Core
{
    /// <summary>
    /// Abstract class defining all Non-Cancellable Request Types
    /// </summary>
    public abstract class NonCancellableClientRequest : IClientRequest
    {
        public string ID { get; }
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
        private object _statelock;

        /// <summary>
        /// Initializes variables to be used for all Cancellable Request Types
        /// </summary>
        public NonCancellableClientRequest(ClaimsPrincipal User, bool isLoggingEnabled = true)
        {
            this.ID = Guid.NewGuid().ToString();
            this.User = User;
            this.ResponseSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            this.State = State.Received;
            this.Message = string.Empty;
            this.StackTrace = string.Empty;

            _statelock = new object();

            if (isLoggingEnabled)
            {
                this.FolderPath = LogManager.CreateRequestFolder(ID);
            }
            else
            {
                this.FolderPath = string.Empty;
            }
        }

        public void SetStatus(State responseType, string message = "", Exception? e = null)
        {
            lock (_statelock)
            {
                if (State == State.Cancelled)
                    throw new InvalidOperationException($"Cannot cancel {this.GetType().Name} type");

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

        public void IsCancellationRequested()
        {
            return; // Nothing occurs
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
                this.SetStatus(State.Cancelled, "Cancelled");
            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, $"Failure (State: {State})", e);
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
            if (Logger != null)
                Logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            if (Logger != null)
                Logger.LogWarning(message);
        }

        public void LogError(string message)
        {
            if (Logger != null)
                Logger.LogError(message);
        }

        public void LogCritical(string message)
        {
            if (Logger != null)
                Logger.LogCritical(message);
        }

        /// <summary>
        /// Flush all logs
        /// </summary>
        private void Flush()
        {
            if (Logger != null)
                Logger.Flush();
        }
    }
}
