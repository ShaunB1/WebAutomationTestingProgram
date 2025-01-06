using System.Text;
using Azure.Identity;
using Microsoft.Graph;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Globalization;
using AutomationTestingProgram.Models.Settings;
using AutomationTestingProgram.Backend.Helpers;

namespace AutomationTestingProgram.Services
{
    public static class PasswordResetService
    {
        private static readonly MicrosoftGraphSettings _settings;
        private static readonly LockManager<string> _lockManager;
        private static readonly SemaphoreSlim _limit;
        private static HttpClient _httpClient;

        /*
         *  Concurrency issues -> what if resetting the same email at the same time??
         *  To fix: Must add thread safety
         *  
         *  Complete PasswordResetService and AzureKeyVaultService once working on login.cs
         * 
         */

        static PasswordResetService()
        {
            _settings = AppConfiguration.GetSection<MicrosoftGraphSettings>("MicrosoftGraph");
            _lockManager = new LockManager<string>(_settings.Limit);
        }

        public static void Initialize(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public static async Task ResetPassword<T>(ILogger<T> Logger, string email)
        {
            try
            {
                Logger.LogInformation("Waiting to start Password Reset");
                await _lockManager.AquireLockAsync(email);

                await AzureKeyVaultService.CheckAzureKVAccount(Logger, email);

                string emailTime = DateTime.UtcNow.ToString("O");
                await RequestOTP(Logger, email);

                // Wait 20 seconds for reset email to be sent
                Logger.LogInformation($"Waiting for 20 seconds for reset email");
                await Task.Delay(20000);

                string OTP = await GetOTPFromEmail(Logger, email, emailTime);

                await RequestPasswordReset(Logger, email, OTP);

                try
                {
                    await AzureKeyVaultService.UpdateKvSecret(Logger, email);
                }
                catch (Exception e)
                {
                    throw new Exception("Password successfully updated in OPS BPS but failed to update in Azure Key Vault\n" +
                            "Suggestion: Sync passwords manually if necessary.\n" + e.Message);
                }

                Logger.LogInformation("Password successfully reset in OPS BPS and updated in Azure Key Vault.");
            }
            finally
            {
                _lockManager.ReleaseLock(email);
            }            
        }

        // Make a password reset request on OPS BPS
        private static async Task RequestOTP<T>(ILogger<T> Logger, string email)
        {
            Logger.LogInformation($"Requesting OTP from OPS BPS");
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
                    Logger.LogInformation($"OTP request successful");
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
        private static async Task<string> GetOTPFromEmail<T>(ILogger<T> Logger, string email, string emailTime)
        {
            string emailText = string.Empty;

            // Find the email containing the OTP
            try
            {
                Logger.LogInformation("Retrieving PIN from reset email");
                
                UsernamePasswordCredential credential = new UsernamePasswordCredential(_settings.GraphEmail, _settings.GraphPassword, _settings.GraphTenantId, _settings.GraphClientId);
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
                    Logger.LogInformation("PIN successfully retrieved.");
                    return pin;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to find OTP in email. {ex.Message}");
            }
        }

        private static async Task RequestPasswordReset<T>(ILogger<T> Logger, string email, string OTP)
        {
            Logger.LogInformation($"Requesting password reset from OPS BPS");
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
                    Logger.LogInformation($"Password reset request successful");
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
