using AutomationTestingProgram.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Services;

public class AzureKeyVaultService
{
    private readonly AzureKeyVaultSettings _azureKeyVaultSettings;
    private string vault;
    private string clientId;
    private string tenantId;
    public AzureKeyVaultService(IOptions<AzureKeyVaultSettings> azureKeyVaultSettings)
    {
        _azureKeyVaultSettings = azureKeyVaultSettings.Value;
        vault = _azureKeyVaultSettings.CredentialVault;
        clientId = _azureKeyVaultSettings.KeyVaultClientId;
        tenantId = _azureKeyVaultSettings.KeyVaultTenantId;
    }

    public string GetKvSecret(string secretName)
    {
        Console.WriteLine("Attempting to read Key Vault secret key for " + secretName);
        try
        {
            using (var httpClient = new HttpClient(new HttpClientHandler()))
            {
                var clientOptions = new SecretClientOptions
                {
                    Transport = new HttpClientTransport(httpClient),
                };

                SecretClient client = GetAzureClient(clientOptions);

                KeyVaultSecret secret = client.GetSecret(secretName.Replace("@", "--").Replace(".", "-").Replace("_", "---").ToLower());
                return secret.Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return $"Error retrieving secret: {ex.Message} {ex.StackTrace}";
        }
    }

    private SecretClient GetAzureClient(SecretClientOptions clientOptions)
    {
        SecretClient client = null;
        KeyVaultSecret secret = null;
        string vault = _azureKeyVaultSettings.CredentialVault;
        string keyVaultUrl = $"https://{vault}.vault.azure.net/";

        try
        {
            client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential(), clientOptions);
            return client;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error connecting to Azure Key Vault: {ex.Message}", ex);
        }
    }
}
