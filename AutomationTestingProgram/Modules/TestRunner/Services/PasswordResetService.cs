﻿using System.Text;
using Azure.Identity;
using Microsoft.Graph;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Globalization;
using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class PasswordResetService
    {
        private readonly string _graphClientID;
        private readonly string _graphTenantID;
        private readonly string _graphEmail;

        private readonly LockManager<string> _lockManager;
        private readonly HttpClient _httpClient;
        private readonly AzureKeyVaultService _azureKeyVaultService;

        public PasswordResetService(IOptions<MicrosoftGraphSettings> options, IHttpClientFactory httpClientFactory, AzureKeyVaultService azureKeyVaultService)
        {
            MicrosoftGraphSettings settings = options.Value;
            _graphClientID = settings.GraphClientId;
            _graphTenantID = settings.GraphTenantId;
            _graphEmail = settings.GraphEmail;

            _lockManager = new LockManager<string>(settings.Limit);
            _httpClient = httpClientFactory.CreateClient("JsonClient");
            _azureKeyVaultService = azureKeyVaultService;
        }

        public async Task ResetPassword(Func<string, Task> Log, string email)
        {
            try
            {
                await Log("Waiting to start Password Reset");
                await _lockManager.AquireLockAsync(email);

                await _azureKeyVaultService.CheckAzureKVAccount(Log, email);

                string emailTime = DateTime.UtcNow.ToString("O");
                await RequestOTP(Log, email);

                // Wait 20 seconds for reset email to be sent
                await Log($"Waiting for 20 seconds for reset email");
                await Task.Delay(20000);

                string OTP = await GetOTPFromEmail(Log, email, emailTime);

                await RequestPasswordReset(Log, email, OTP);

                try
                {
                    await _azureKeyVaultService.UpdateKvSecret(Log, email);
                }
                catch (Exception e)
                {
                    throw new Exception("Password successfully updated in OPS BPS but failed to update in Azure Key Vault\n" +
                            "Suggestion: Sync passwords manually if necessary.\n" + e.Message);
                }

                await Log("Password successfully reset in OPS BPS and updated in Azure Key Vault.");
            }
            finally
            {
                _lockManager.ReleaseLock(email);
            }
        }

        // Make a password reset request on OPS BPS
        private async Task RequestOTP(Func<string, Task> Log, string email)
        {
            await Log($"Requesting OTP from OPS BPS");
            string forgotPasswordURL = "https://stage.login.security.gov.on.ca/ciam/bps/public/forgotpassword/";

            var jsonBody = $"{{\"email\":\"{email}\"}}";
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(forgotPasswordURL, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                var responseObject = JsonSerializer.Deserialize<PasswordResetResponse>(responseBody);

                if (response.IsSuccessStatusCode && responseObject != null && responseObject.result == 0)
                {
                    await Log($"OTP request successful");
                }
                else
                {
                    throw new Exception($"OTP request failed with status code {response.StatusCode}\n{responseBody}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"OTP request failed. {ex.Message}");
            }
        }

        // Retrieve one-time password from uft@ontario.ca
        private async Task<string> GetOTPFromEmail(Func<string, Task> Log, string email, string emailTime)
        {
            string emailText = string.Empty;
            string graphPassword = await _azureKeyVaultService.GetKvSecret(Log, _graphEmail);

            // Find the email containing the OTP
            try
            {
                await Log("Retrieving PIN from reset email");

                UsernamePasswordCredential credential = new UsernamePasswordCredential(_graphEmail, graphPassword, _graphTenantID, _graphClientID);
                GraphServiceClient graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

                // Get the top 20 most recent emails that were sent after emailTime - the time we requested OTP
                var filterQuery = $"receivedDateTime ge {emailTime}";
                var messages = await graphClient.Me.Messages.GetAsync((config) =>
                {
                    config.QueryParameters.Filter = filterQuery;
                    config.QueryParameters.Top = 20;
                    config.QueryParameters.Select = new[] { "body", "receivedDateTime", "toRecipients" };
                    config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                });

                // Get the most recent email with recipient {email} variable
                var filteredMessages = messages?.Value
                    ?.Where(message => message.ToRecipients != null &&
                       message.ToRecipients.Any(recipient =>
                           recipient.EmailAddress.Address.Equals(email, StringComparison.OrdinalIgnoreCase)))
                    ?.OrderByDescending(message => message.ReceivedDateTime);

                var mostRecentMessage = filteredMessages?.FirstOrDefault();
                if (mostRecentMessage != null)
                {
                    emailText = mostRecentMessage.Body.Content;
                }
                if (emailText == string.Empty)
                {
                    throw new Exception("No reset password email was found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to find email containing OTP. {ex.Message}");
            }

            // Find OTP from the email text. Searches for 5 digits or more in between bodies of text
            try
            {
                string pattern = @"\b\d{5,}\b";
                MatchCollection matches = Regex.Matches(emailText, pattern);
                string pin = string.Empty;
                foreach (Match match in matches)
                {
                    pin = match.Value;
                }
                if (string.IsNullOrEmpty(pin))
                {
                    throw new Exception("Pin was not found in email");
                }
                else
                {
                    await Log("PIN successfully retrieved.");
                    return pin;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to find OTP in email. {ex.Message}");
            }
        }

        private async Task RequestPasswordReset(Func<string, Task> Log, string email, string OTP)
        {
            await Log($"Requesting password reset from OPS BPS");
            string resetPasswordURL = "https://stage.login.security.gov.on.ca/ciam/bps/public/passwordreset/";

            string newPassword = $"OPS{DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture)}!";
            var jsonBody = $"{{\"email\":\"{email}\",\"newPassword\":\"{newPassword}\",\"confirmNewPassword\":\"{newPassword}\",\"otp\":\"{OTP}\"}}";
            var content = new StringContent(jsonBody, null, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(resetPasswordURL, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                var responseObject = JsonSerializer.Deserialize<PasswordResetResponse>(responseBody);

                if (response.IsSuccessStatusCode && responseObject != null && responseObject.result == 0)
                {
                    await Log($"Password reset request successful");
                }
                else
                {
                    throw new Exception($"Password reset request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Password reset request failed. {ex.Message}");
            }
        }

        public class PasswordResetResponse
        {
            public int result { get; set; }
            public int error { get; set; }
        }
    }
}
