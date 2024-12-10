using System.Text.Json.Nodes;
using System.Text;
using AutomationTestingProgram.Models;
using Microsoft.Extensions.Options;
using Azure.Identity;
using Microsoft.Graph;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using System.Text.Json;
using System.Globalization;

namespace AutomationTestingProgram.Services
{
    public class PasswordResetService
    {
        private readonly AzureKeyVaultService _azureKeyVaultService;
        private static readonly HttpClient _client;
        private string _graphClientId;
        private string _graphTenantId;
        private string _graphEmail;
        private string _graphPassword;

        public PasswordResetService(IOptions<MicrosoftGraphSettings> microsoftGraphSettings, AzureKeyVaultService azureKeyVaultService)
        {
            _azureKeyVaultService = azureKeyVaultService;
            _graphClientId = microsoftGraphSettings.Value.GraphClientId;
            _graphTenantId = microsoftGraphSettings.Value.GraphTenantId;
            _graphEmail = microsoftGraphSettings.Value.GraphEmail;
            _graphPassword = microsoftGraphSettings.Value.GraphPassword;
        }

        static PasswordResetService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        }

        public async Task<(bool success, string message)> ResetPassword(string email)
        {
            var result = await _azureKeyVaultService.CheckAzureKVAccount(email);
            Console.WriteLine($"{email}: {result.message}");
            if (!result.success)
            {
                return (false, result.message);
            }

            string emailTime = DateTime.UtcNow.ToString("O");
            result = await RequestOTP(email);
            Console.WriteLine($"{email}: {result.message}");
            if (!result.success)
            {
                return (false, result.message);
            }

            // Wait 20 seconds for reset email to be sent
            Console.WriteLine($"{email}: Waiting for 20 seconds for reset email");
            await Task.Delay(20000);

            result = await GetOTPFromEmail(email, emailTime);
            Console.WriteLine($"{email}: {result.message}");
            if (!result.success)
            {
                return (false, result.message);
            }
            string OTP = result.message;

            result = await RequestPasswordReset(email, OTP);
            Console.WriteLine($"{email}: {result.message}");
            if (!result.success)
            {
                return (false, result.message);
            }

            result = await _azureKeyVaultService.UpdateKvSecret(email);
            Console.WriteLine($"{email}: {result.message}");
            if (!result.success)
            {
                return (false, "Password successfully updated in OPS BPS but failed to update in Azure Key Vault\n" +
                        "Suggestion: Sync passwords manually if necessary.\n" + result.message);
            }
            return (true, "Password successfully reset in OPS BPS and updated in Azure Key Vault.");
        }

        // Make a password reset request on OPS BPS
        private async Task<(bool success, string message)> RequestOTP(string email)
        {
            Console.WriteLine($"{email}: Requesting OTP from OPS BPS");
            string forgotPasswordURL = "https://stage.login.security.gov.on.ca/ciam/bps/public/forgotpassword/";

            var jsonBody = $"{{\"email\":\"{email}\"}}";
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _client.PostAsync(forgotPasswordURL, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                var responseObject = JsonSerializer.Deserialize<PasswordResetResponse>(responseBody);

                if (response.IsSuccessStatusCode && responseObject != null && responseObject.result == 0)
                {
                    return (true, $"OTP request successful\nResponse:{responseBody}");
                }
                else
                {
                    return (false, $"OTP request failed with status code {response.StatusCode}\n{responseBody}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"OTP request failed. {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Retrieve one-time password from uft@ontario.ca
        private async Task<(bool success, string message)> GetOTPFromEmail(string email, string emailTime)
        {
            string emailText = string.Empty;

            // Find the email containing the OTP
            try
            {
                UsernamePasswordCredential credential = new UsernamePasswordCredential(_graphEmail, _graphPassword, _graphTenantId, _graphClientId);
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
                    return (false, "No reset password email was found");
                }
                Console.WriteLine($"{email} password reset email text\n{emailText}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to find email containing OTP. {ex.Message}\n{ex.StackTrace}");
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
                    return (false, "Pin was not found in email");
                }
                else
                {
                    return (true, pin);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Failed to find OTP in email. {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task<(bool success, string message)> RequestPasswordReset(string email, string OTP)
        {
            Console.WriteLine($"{email}: Requesting password reset from OPS BPS");
            string resetPasswordURL = "https://stage.login.security.gov.on.ca/ciam/bps/public/passwordreset/";

            string newPassword = $"OPS{DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture)}!";
            var jsonBody = $"{{\"email\":\"{email}\",\"newPassword\":\"{newPassword}\",\"confirmNewPassword\":\"{newPassword}\",\"otp\":\"{OTP}\"}}";
            var content = new StringContent(jsonBody, null, "application/json");

            try
            {
                HttpResponseMessage response = await _client.PostAsync(resetPasswordURL, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                var responseObject = JsonSerializer.Deserialize<PasswordResetResponse>(responseBody);

                if (response.IsSuccessStatusCode && responseObject != null && responseObject.result == 0)
                {
                    return (true, $"Password reset request successful\nResponse:{responseBody}");
                }
                else
                {
                    return (false, $"Password reset request failed with status code {response.StatusCode}\n{responseBody}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Password reset request failed. {ex.Message}\n{ex.StackTrace}");
            }
        }

        public class PasswordResetResponse
        {
            public int result { get; set; }
            public int error { get; set; }
        }
    }
}
