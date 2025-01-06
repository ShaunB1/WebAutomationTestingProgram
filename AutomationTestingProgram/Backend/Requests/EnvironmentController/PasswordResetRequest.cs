using AutomationTestingProgram.Services;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Math;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Backend
{
    /// <summary>
    /// Request to reset password for an email
    /// </summary>
    public class PasswordResetRequest : IClientRequest
    {   
        public string ID { get; }
        [JsonIgnore]
        public ClaimsPrincipal User { get; }
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
        public ILogger<PasswordResetRequest> Logger { get; }

        /// <summary>
        /// The email linked with the request
        /// </summary>
        public string Email { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetRequest"/> class.
        /// </summary>
        public PasswordResetRequest(ClaimsPrincipal User, string Email)
        {
            ID = Guid.NewGuid().ToString();
            this.User = User;
            ResponseSource = new TaskCompletionSource();
            State = State.Received;
            StateLock = new object();
            CancellationTokenSource = new CancellationTokenSource();
            Message = string.Empty;
            FolderPath = LogManager.CreateRequestFolder(ID);

            CustomLoggerProvider provider = new CustomLoggerProvider(FolderPath);
            Logger = provider.CreateLogger<PasswordResetRequest>()!;

            this.Email = Email;
        }

        public void SetStatus(State responseType, string message = "", Exception? e = null)
        {
            lock (StateLock)
            {

                if (ResponseSource!.Task.IsCompleted)
                    return; // DO nothing if already complete

                if (responseType == State.Cancelled)
                    throw new Exception("Cannot cancel this Request!");

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
                    case State.Rejected:
                        Logger.LogInformation($"State: {State}\nMessage: {Message}");
                        this.Flush();
                        break;
                    default:
                        Logger.LogInformation($"State: {State}\nMessage: {Message}");
                        break;
                }
            }
        }

        public async Task IsCancellationRequested()
        {
            await Task.CompletedTask;
        }

        public async Task Process()
        {
            try
            {
                this.Validate();

                if (this.ResponseSource!.Task.IsCompleted) // Skip Execute if Validate completed request
                    return;

                await this.Execute();
            }
            catch (Exception e) // Unexpected exception.
            {
                Logger.LogError("Unexpected exception occured.");
                this.SetStatus(State.Failure, "Unexpected exception", e);
            }
            finally
            {
                this.Flush();
            }

        }

        /// <summary>
        /// Validate the <see cref="PasswordResetRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        private void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application section (requets sent from sections in the application)
             */

            try
            {
                Logger.LogInformation($"Validating PasswordReset Request (ID: {ID})");

                // Validate permission to access application
                this.SetStatus(State.Validating, $"Validating User Permissions - Team");

                /*
                 * Later implementation:
                 * Validate whether the user has permission to access the email account
                 */

            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Validation Failure", e);
            }
        }

        /// <summary>
        /// Execute the <see cref="PasswordResetRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        private async Task Execute()
        {
            try
            {
                Logger.LogInformation($"Processing PasswordReset Request (ID: {ID}, Email: {Email})");

                await IOManager.TryAquireSlotAsync();
                await PasswordResetService.ResetPassword(Logger, Email);

                this.SetStatus(State.Completed, $"PasswordReset Request (ID: {ID}, Email: {Email}) completed successfully");
            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Processing Failure", e);
            }
            finally
            {
                IOManager.ReleaseSlot();
            }
        }

        /// <summary>
        /// Flush all logs.
        /// </summary>
        private void Flush()
        {
            if (Logger is CustomLogger<PasswordResetRequest> customLogger)
            {
                customLogger.Flush();
            }
        }
    }
}
