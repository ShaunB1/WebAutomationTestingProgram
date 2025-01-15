using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Globalization;
using Azure;
using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule;

public class AzureKeyVaultService
{
    private readonly string _vault;
    private readonly string _cliendID;
    private readonly string _tenantID;
    private readonly string _clientSecret;

    private readonly HttpClient _httpClient;

    public AzureKeyVaultService(IOptions<AzureKeyVaultSettings> options, HttpClient httpClient)
    {
        AzureKeyVaultSettings settings = options.Value;
        _vault = settings.CredentialVault;
        _cliendID = settings.KeyVaultClientId;
        _tenantID = settings.KeyVaultTenantId;
        _clientSecret = settings.KeyVaultClientSecret;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves a KeyVault secret as a string.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve</param>
    /// <returns>A string secret or an error</returns>
    public async Task<string> GetKvSecret(IClientRequest request, string secretName)
    {
        try
        {
            request.LogInfo($"Retrieving KeyVault secret for {secretName}.");

            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient)
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());

            request.LogInfo($"KeyVault secret retrieved successfully");
            return secret.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving secret: {HandleException(ex)}");
        }
    }

    /// <summary>
    /// Updates a KeyVault Secret.
    /// </summary>
    /// <param name="secretName">The name of the secret to update</param>
    /// <returns></returns>
    public async Task UpdateKvSecret(IClientRequest request, string secretName)
    {
        try
        {
            request.LogInfo($"Updating KeyVault secret for {secretName}.");

            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.SetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower(),
                                                     $"OPS{DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture)}!");
            request.LogInfo("Successfully updated secret key");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating secret: {HandleException(ex)}");
        }
    }

    // Before updating a password on OPS BPS, use this function to check that the account is enabled first and also verify connection with Key Vault
    public async Task CheckAzureKVAccount(IClientRequest request, string secretName)
    {
        try
        {
            request.LogInfo($"Verifying account: {secretName} and KeyVault connection");

            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);
            KeyVaultSecret secret;

            try
            {
                secret = await client.GetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving secret: {HandleException(e)}");
            }

            if (secret.Value == "OPS" + DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture) + "!")
            {
                throw new PasswordResetLimitException($"Password for {secretName} was already updated today. Please try again tomorrow.");
            }

            if (secret.Properties.Enabled == true)
            {
                request.LogInfo($"Successfully verified that {secretName} is enabled in Azure Key Vault. Proceeding with password reset");
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
            throw new Exception($"Error connecting to Azure Key Vault: {ex.Message}");
        }
    }

    private SecretClient GetAzureClient(SecretClientOptions clientOptions)
    {
        string keyVaultUrl = $"https://{_vault}.vault.azure.net/";

        try
        {
            SecretClient client = new SecretClient(new Uri(keyVaultUrl), new ClientSecretCredential(_tenantID, _cliendID, _clientSecret), clientOptions);
            return client;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error connecting to Azure Key Vault: {HandleException(ex)}");
        }
    }

    private static string HandleException(Exception e)
    {
        switch (e)
        {
            case RequestFailedException rfe when rfe.Status == 403: return "Client address is not authorized and caller is not a trusted service. Make sure 'Canada Central' is in use.";
            case RequestFailedException rfe when rfe.Status == 404: return "The secret was not found.";
            case RequestFailedException rfe when rfe.Status == 401: return "Unauthorized request.";
            case RequestFailedException rfe when rfe.Status == 400: return "Bad request.";
            case RequestFailedException rfe when rfe.Status == 500: return "Internal server error.";
            case RequestFailedException rfe when rfe.Status == 503: return "Service unavailable.";
            case ArgumentException: return "Invalid argument provided.";
            case TaskCanceledException: return "Task was canceled.";
            case AuthenticationFailedException: return "Authentication with Azure Key Vault failed.";
            case OperationCanceledException: return "Operation was canceled.";
            default: return e.Message;
        }
    }


}
