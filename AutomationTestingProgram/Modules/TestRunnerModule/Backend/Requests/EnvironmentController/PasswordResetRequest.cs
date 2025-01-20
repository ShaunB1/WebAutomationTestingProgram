using System.Security.Claims;
using System.Text.Json.Serialization;
using AutomationTestingProgram.Core;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Request to reset password for an email
    /// </summary>
    public class PasswordResetRequest : NonCancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// The email linked with the request
        /// </summary>
        public string Email { get; }

        private PasswordResetService passwordResetService;


        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetRequest"/> class.
        /// </summary>
        public PasswordResetRequest(ICustomLoggerProvider provider, PasswordResetService service, ClaimsPrincipal User, string Email)
            :base(User)
        {
            Logger = provider.CreateLogger<PasswordResetRequest>(FolderPath);

            this.Email = Email;

            this.passwordResetService = service;
        }        

        /// <summary>
        /// Validate the <see cref="PasswordResetRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application section (requets sent from sections in the application)
             */

            this.SetStatus(State.Validating, $"Validating PasswordReset Request (ID: {ID})");

            // Validate permission to access application
            LogInfo($"Validating User Permissions - Team");

            /*
             * Later implementation:
             * Validate whether the user has permission to access the email account
             */
        }

        /// <summary>
        /// Execute the <see cref="PasswordResetRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            try
            {
                this.SetStatus(State.Processing, $"Processing PasswordReset Request (ID: {ID}, Email: {Email})");

                await IOManager.TryAquireSlotAsync();
                await passwordResetService.ResetPassword(this, Email);

                SetStatus(State.Completed, $"PasswordReset Request (ID: {ID}, Email: {Email}) completed successfully");
            }
            finally
            {
                IOManager.ReleaseSlot();
            }
        }
    }
}
