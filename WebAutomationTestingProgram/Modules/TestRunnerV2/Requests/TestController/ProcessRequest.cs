using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using WebAutomationTestingProgram.Core.Helpers.Requests;
using WebAutomationTestingProgram.Core.Hubs;
using WebAutomationTestingProgram.Core.Services.Logging;
using WebAutomationTestingProgram.Modules.TestRunner.Backend.Helpers;
using WebAutomationTestingProgram.Modules.TestRunner.Models.Requests;
using WebAutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;

namespace WebAutomationTestingProgram.Modules.TestRunner.Requests.TestController
{
    /// <summary>
    /// ProcessRequest is used to configure the request's log paths and test execution.
    /// </summary>
    public class ProcessRequest : CancellableClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }
        public string FileName { get; }
        public string Browser { get; }
        public string BrowserVersion { get; }
        public string Environment { get; }
        public double StepDelay { get; }

        [JsonIgnore]
        private readonly AsyncPauseControl _pauseControl;

        [JsonIgnore]
        private readonly PlaywrightObject _playwright;

        [JsonIgnore]
        private readonly IHubContext<TestHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRequest"/> class.
        /// Instance is associated with a file and the specified browser type and version.
        /// </summary>
        /// <param name="File">The file to be processed in the request.</param>
        /// <param name="Type">The type of the browser (e.g., "Chrome", "Firefox") that will handle the request.</param>
        /// <param name="Version">The version of the browser (e.g., "91", "93") that will be used to process the request.</param>
        /// <param name="provider">The custom logger.</param>
        /// <param name="hubContext">Handles the websocket clients.</param>
        /// <param name="playwright">The playwright object to call playwright functions.</param>
        public ProcessRequest(
            ICustomLoggerProvider provider,
            IHubContext<TestHub> hubContext,
            PlaywrightObject playwright,
            ClaimsPrincipal User,
            string guid,
            ProcessRequestModel model) : base(User, isLoggingEnabled:true, guid)
        {            
            Logger = provider.CreateLogger<ProcessRequest>(FolderPath);
            FileName = model.File.FileName;
            Browser = model.Browser;
            BrowserVersion = model.BrowserVersion;
            Environment = model.Environment;
            StepDelay = model.Delay;
            _playwright = playwright;
            _hubContext = hubContext;
            _pauseControl = new AsyncPauseControl(Logger, CancelToken);
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

            SetStatus(State.Validating, $"Validating Process Request (ID: {Id}, BrowserType: {Browser}," +
                    $" BrowserVersion: {BrowserVersion}, Environment: {Environment})");

            // Validate permission to access team
            LogInfo($"Validating User Permissions - Team");
        }

        /// <summary>
        /// Execute the <see cref="ProcessRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            SetStatus(State.Processing, $"Processing Process Request (ID: {Id}, BrowserType: {Browser}," +
                    $" BrowserVersion: {BrowserVersion}, Environment: {Environment})");

            IsCancellationRequested();

            await _playwright.ProcessRequestAsync( this );

            SetStatus(State.Completed, $"Process Request (ID: {Id}, BrowserType: {Browser}," +
                $" BrowserVersion: {BrowserVersion}, Environment: {Environment}) completed successfully");

            await _hubContext.Clients.Group(Id).SendAsync("RunFinished", Id, $"Test Run: {Id} has completed successfully");
        }

        /// <summary>
        /// Pause the request. 
        /// Will wait until unpaused, cancelled, or 10 minute timeout.
        /// </summary>
        public void Pause()
        {
            _pauseControl.Pause(); // Set the event to non-signaled state (pause)

        }

        /// <summary>
        /// Unpause the request.
        /// Will continue running the request.
        /// </summary>
        public void Unpause()
        {
            _pauseControl.UnPause(); // Set the event to signaled state (unpause)
        }

        public async Task IsPauseRequested(Func<string, Task> Log)
        {
            await _pauseControl.WaitAsync(Log);
        }

    }
}
