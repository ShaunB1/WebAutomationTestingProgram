using System.Security.Claims;
using System.Text.Json.Serialization;
using WebAutomationTestingProgram.Core.Helpers;
using WebAutomationTestingProgram.Core.Helpers.Requests;
using WebAutomationTestingProgram.Core.Services.Logging;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Requests;
using WebAutomationTestingProgram.Modules.TestRunner.Services;

namespace WebAutomationTestingProgram.Modules.TestRunner.Backend.Requests.EnvironmentController
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

        [JsonIgnore]
        private PasswordResetService passwordResetService;


        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetRequest"/> class.
        /// </summary>
        public PasswordResetRequest(ICustomLoggerProvider provider, PasswordResetService service, ClaimsPrincipal User, PasswordResetRequestModel model)
            :base(User)
        {
            Logger = provider.CreateLogger<PasswordResetRequest>(FolderPath);

            this.Email = model.Email;

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

            this.SetStatus(State.Validating, $"Validating PasswordReset Request (ID: {Id})");

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
                this.SetStatus(State.Processing, $"Processing PasswordReset Request (ID: {Id}, Email: {Email})");

                await IOManager.TryAquireSlotAsync();
                await passwordResetService.ResetPassword(Log, Email);

                SetStatus(State.Completed, $"PasswordReset Request (ID: {Id}, Email: {Email}) completed successfully");
            }
            finally
            {
                IOManager.ReleaseSlot();
            }
        }
    }
}
