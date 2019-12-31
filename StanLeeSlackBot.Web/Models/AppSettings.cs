namespace StanLeeSlackBot.Web.Models
{
    public class AppSettings
    {
        public ApplicationInsightsSettings ApplicationInsights { get; set; }
        public AzureSettings Azure { get; set; }
        public string BuildNumber { get; set; }
        public GoogleCustomSearchSettings GoogleCustomSearch { get; set; }
        public string IpStackApiKey { get; set; }
        public SlackSettings Slack { get; set; }
    }

    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }
    }

    public class AzureSettings
    {
        public KeyVaultSettings KeyVault { get; set; }
        public StorageSettings Storage { get; set; }

        public class KeyVaultSettings
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string DataProtectionSecret { get; set; }
            public string EncryptionKey { get; set; }
        }

        public class StorageSettings
        {
            public string ConnectionString { get; set; }
            public string DataProtectionContainer { get; set; }
        }
    }

    public class GoogleCustomSearchSettings
    {
        public string ApiKey { get; set; }
        public string DcComicsCx { get; set; }
        public string MarvelCx { get; set; }
    }

    public class SlackSettings
    {
        public string ApiToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SigningSecret { get; set; }
        public string VerificationToken { get; set; }
    }
}
