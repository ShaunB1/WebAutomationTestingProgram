namespace AutomationTestingProgram.Models.Settings
{
    public class AzureKeyVaultSettings
    {
        public string CredentialVault { get; set; }
        public string KeyVaultClientId { get; set; }
        public string KeyVaultTenantId { get; set; }
        public string KeyVaultClientSecret { get; set; }
    }
}
