
using AutomationTestingProgram.Core;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Request to process a test file using playwright
    /// </summary>
    public class ProcessRequest : CancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

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
        public ProcessRequest(ICustomLoggerProvider provider, ClaimsPrincipal User, IFormFile File, string Type, string Version, string Environment)
            : base(User)
        {            
            this.Logger = provider.CreateLogger<ProcessRequest>(FolderPath);
            
            this.File = File;
            this.BrowserType = Type;
            this.BrowserVersion = Version;
            this.Environment = Environment;
        }

        /// <summary>
        /// Validate the <see cref="ProcessRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application: Validated with Authorize
             * - User has permission to access application team/group
             */

            try
            {
                this.SetStatus(State.Validating, $"Validating Process Request (ID: {ID}, BrowserType: {BrowserType}," +
                    $" BrowserVersion: {BrowserVersion}, Environment: {Environment})");

                // Validate permission to access team
                LogInfo($"Validating User Permissions - Team");

            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Validation Failure", e);
            }
        }

        /// <summary>
        /// Execute the <see cref="ProcessRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            try
            {
                this.SetStatus(State.Processing, $"Processing Process Request (ID: {ID}, BrowserType: {BrowserType}," +
                    $" BrowserVersion: {BrowserVersion}, Environment: {Environment})");

                IsCancellationRequested();

                for (int i = 0; i <= 5; i++)
                {
                    // Check if cancellation requested

                    IsCancellationRequested();
                    await Task.Delay(20000);
                    LogInfo($"{i}");
                }

                this.SetStatus(State.Completed, $"Process Request (ID: {ID}, BrowserType: {BrowserType}," +
                    $" BrowserVersion: {BrowserVersion}, Environment: {Environment}) completed successfully");
            }
            catch (Exception e)
            {
                this.SetStatus(State.Failure, "Processing Failure", e);
            }
        }
    }
}
