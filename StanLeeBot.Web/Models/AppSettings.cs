﻿namespace StanLeeBot.Web.Models
{
    public class AppSettings
    {
        public ApplicationInsightsSettings ApplicationInsights { get; set; }
        public BabouAuthKeySettings BabouAuthKeys { get; set; }
        public AzureSettings Azure { get; set; }
        public string BuildNumber { get; set; }
        public EmailSenderSettings EmailSender { get; set; }
        public GoogleCustomSearchSettings GoogleCustomSearch { get; set; }
        public string IpStackApiKey { get; set; }
        public SlackSettings Slack { get; set; }
        public TelegramSettings Telegram { get; set; }
        public string UrlShortenerEndpoint { get; set; }
    }

    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }
    }

    public class BabouAuthKeySettings
    {
        public string Slack { get; set; }
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

    public class EmailSenderSettings
    {
        public string ApiKey { get; set; }
        public string Domain { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
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
        public string Scopes { get; set; }
        public string SigningSecret { get; set; }
        public string VerificationToken { get; set; }
    }

    public class TelegramSettings
    {
        public string ApiToken { get; set; }
    }
}
