using AutomationTestingProgram.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Playwright;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using AutomationTestingProgram.Models.Settings;
using System.Net.Http;

namespace AutomationTestingProgram.Services;

public class AzureKeyVaultService
{
    private readonly string _vault;
    private readonly string _clientId;
    private readonly string _tenantId;
    private readonly string _clientSecret;
    private readonly HttpClient _httpClient;

    public AzureKeyVaultService(IOptions<AzureKeyVaultSettings> azureKeyVaultSettings, HttpClient httpClient)
    {
        var azureConfig = azureKeyVaultSettings.Value;
        _vault = azureConfig.CredentialVault;
        _clientId = azureConfig.KeyVaultClientId;
        _tenantId = azureConfig.KeyVaultTenantId;
        _clientSecret = azureConfig.KeyVaultClientSecret;

        _httpClient = httpClient;
    }

    public async Task<(bool success, string message)> GetKvSecret(string secretName)
    {
        try
        {
            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());
            return (true, secret.Value);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving secret: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public async Task<(bool success, string message)> UpdateKvSecret(string secretName)
    {
        try
        {
            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.SetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower(),
                                                     $"OPS{DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture)}!");
            return (true, "Successfully updated secret key for " + secretName);
        }
        catch (Exception ex)
        {
            return (false, $"Error updating secret: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Before updating a password on OPS BPS, use this function to check that the account is enabled first and also verify connection with Key Vault
    public async Task<(bool success, string message)> CheckAzureKVAccount(string secretName)
    {
        try
        {
            var clientOptions = new SecretClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };

            SecretClient client = GetAzureClient(clientOptions);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());

            if (secret.Value == "OPS" + DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture) + "!")
            {
                return (false, $"Error: Password for {secretName} was already updated today");
            }

            if (secret.Properties.Enabled == true)
            {
                return (true, $"Successfully verified that {secretName} is enabled in Azure Key Vault. Proceeding with password reset");
            }
            else if (secret.Properties.Enabled == false)
            {
                return (false, $"Error: {secretName} is disabled in Azure Key Vault");
            }
            else
            {
                return (false, $"Error: The status of {secretName} is unknown (enabled property is null)");
            }

        }
        catch (Exception ex)
        {
            return (false, $"Error connecting to Azure Key Vault: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private SecretClient GetAzureClient(SecretClientOptions clientOptions)
    {
        string keyVaultUrl = $"https://{_vault}.vault.azure.net/";

        try
        {
            SecretClient client = new SecretClient(new Uri(keyVaultUrl), new ClientSecretCredential(_tenantId, _clientId, _clientSecret), clientOptions);
            return client;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error connecting to Azure Key Vault: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
