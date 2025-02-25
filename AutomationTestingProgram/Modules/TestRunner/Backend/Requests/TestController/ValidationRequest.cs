using System.Security.Claims;
using System.Text.Json.Serialization;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Helpers.Requests;
using AutomationTestingProgram.Core.Services.Logging;

namespace AutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController
{
    /// <summary>
    /// Request to validate a test file.
    /// </summary>
    public class ValidationRequest : CancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationRequest"/> class.
        /// Instance is associated with a file.
        /// </summary>
        /// <param name="File">The file to be validated in the request.</param>
        public ValidationRequest(ICustomLoggerProvider provider, ClaimsPrincipal User)
            :base(User)
        {
            this.Logger = provider.CreateLogger<ValidationRequest>(FolderPath);
        }

        /// <summary>
        /// Validate the <see cref="ValidationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application team/group
             */

            this.SetStatus(State.Validating, $"Validating Process Request (ID: {Id})");

            // Validate permission to access team
            LogInfo($"Validating User Permissions - Team");
        }

        /// <summary>
        /// Execute the <see cref="ValidationRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            this.SetStatus(State.Processing, $"Processing Validation Request (ID: {Id})");

            IsCancellationRequested();

            for (int i = 0; i <= 5; i++)
            {
                await Task.Delay(20000, CancelToken);
                Logger.LogInformation($"{i}");
            }

            this.SetStatus(State.Completed, $"Validation Request (ID: {Id}) completed successfully");
        }
    }
}
