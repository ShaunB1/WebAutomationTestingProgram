﻿using AutomationTestingProgram.Core;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Request to retrieve secret keys
    /// </summary>
    public class SecretKeyRetrievalRequest : NonCancellableClientRequest, IClientRequest
    {
        [JsonIgnore]
        protected override ICustomLogger Logger { get; }

        /// <summary>
        /// The email linked with the request
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// The secret found with this request
        /// </summary>
        public string SecretKey { get; private set; }

        [JsonIgnore]
        private AzureKeyVaultService azureKeyVaultService;


        /// <summary>
        /// Initializes a new instance of the <see cref="SecretKeyRetrievalRequest"/> class.
        /// </summary>
        public SecretKeyRetrievalRequest(ICustomLoggerProvider provider, AzureKeyVaultService service, ClaimsPrincipal User, SecretKeyRetrievalRequestModel model)
            :base(User)
        {
            Logger = provider.CreateLogger<SecretKeyRetrievalRequest>(FolderPath);

            this.Email = model.Email;
            this.SecretKey = string.Empty;

            this.azureKeyVaultService = service;
        }

        /// <summary>
        /// Validate the <see cref="SecretKeyRetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override void Validate()
        {
            /*
             * VALIDATION:
             * - User has permission to access application
             * - User has permission to access application section (requets sent from sections in the application)
             */

            SetStatus(State.Validating, $"Validating SecretKeyRetrieval Request (ID: {ID}, Email: {Email})");

            // Validate permission to access application
            LogInfo($"Validating User Permissions - Team");

            /*
             * Later implementation:
             * Validate whether the user has permission to access the email account.
             */
        }

        /// <summary>
        /// Execute the <see cref="SecretKeyRetrievalRequest"/>.
        /// View inner documentation on specifics.  
        /// </summary>
        protected override async Task Execute()
        {
            try
            {
                SetStatus(State.Processing, $"Processing SecretKeyRetrieval Request (ID: {ID}, Email: {Email})");


                await IOManager.TryAquireSlotAsync();
                SecretKey = await azureKeyVaultService.GetKvSecret(LogInfoAsync, Email);

                SetStatus(State.Completed, $"SecretKeyRetrieval Request (ID: {ID}, Email: {Email}) completed successfully");
            }
            finally
            {
                IOManager.ReleaseSlot();
            }
        }
    }
}
