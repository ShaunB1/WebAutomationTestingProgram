using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Globalization;
using AutomationTestingProgram.Models.Settings;
using AutomationTestingProgram.Models.Exceptions;
using AutomationTestingProgram.Backend.Helpers;
using Microsoft.Graph.Models;

namespace AutomationTestingProgram.Services;

public static class AzureKeyVaultService
{
    private static readonly AzureKeyVaultSettings _settings;
    private static HttpClient _httpClient;

    static AzureKeyVaultService()
    {
        _settings = AppConfiguration.GetSection<AzureKeyVaultSettings>("AzureKeyVault");
    }

    public static void Initialize(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves a KeyVault secret as a string.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve</param>
    /// <returns>A string secret or an error</returns>
    public static async Task<string> GetKvSecret<T>(ILogger<T> Logger, string secretName)
    {
        try
        {
            Logger.LogInformation($"Retrieving KeyVault secret for {secretName}.");

            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
                Diagnostics =
                {
                    IsLoggingEnabled = false, 
                    IsDistributedTracingEnabled = false 
                }
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());

            Logger.LogInformation($"KeyVault secret retrieved successfully");
            return secret.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving secret: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a KeyVault Secret.
    /// </summary>
    /// <param name="secretName">The name of the secret to update</param>
    /// <returns></returns>
    public static async Task UpdateKvSecret<T>(ILogger<T> Logger, string secretName)
    {
        try
        {
            Logger.LogInformation($"Updating KeyVault secret for {secretName}.");
            
            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.SetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower(),
                                                     $"OPS{DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture)}!");
            Logger.LogInformation("Successfully updated secret key");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating secret: {ex.Message}");
        }
    }

    // Before updating a password on OPS BPS, use this function to check that the account is enabled first and also verify connection with Key Vault
    public static async Task CheckAzureKVAccount<T>(ILogger<T> Logger, string secretName)
    {
        try
        {
            Logger.LogInformation($"Verifying account: {secretName} and KeyVault connection");

            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());

            if (secret.Value == "OPS" + DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture) + "!")
            {
                throw new PasswordResetLimitException($"Password for {secretName} was already updated today. Please try again tomorrow.");
            }

            if (secret.Properties.Enabled == true)
            {
                Logger.LogInformation($"Successfully verified that {secretName} is enabled in Azure Key Vault. Proceeding with password reset");
            }
            else if (secret.Properties.Enabled == false)
            {
                throw new Exception($"{secretName} is disabled in Azure Key Vault");
            }
            else
            {
                throw new Exception($"The status of {secretName} is unknown (enabled property is null)");
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Error connecting to Azure Key Vault: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static SecretClient GetAzureClient(SecretClientOptions clientOptions)
    {
        string keyVaultUrl = $"https://{_settings.CredentialVault}.vault.azure.net/";

        try
        {
            SecretClient client = new SecretClient(new Uri(keyVaultUrl), new ClientSecretCredential(_settings.KeyVaultTenantId, _settings.KeyVaultClientId, _settings.KeyVaultClientSecret), clientOptions);
            return client;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error connecting to Azure Key Vault: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
