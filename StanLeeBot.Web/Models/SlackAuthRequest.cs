using Newtonsoft.Json;

namespace StanLeeBot.Web.Models
{
    public static class SlackAuthRequest
    {
        public class AuthRequest
        {
            [JsonProperty("ok")]
            public bool Ok { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }

            [JsonProperty("user_id")]
            public string UserId { get; set; }

            [JsonProperty("team_id")]
            public string TeamId { get; set; }

            [JsonProperty("enterprise_id")]
            public object EnterpriseId { get; set; }

            [JsonProperty("team_name")]
            public string TeamName { get; set; }

            [JsonProperty("incoming_webhook")]
            public IncomingWebhook IncomingWebhook { get; set; }

            [JsonProperty("bot")]
            public Bot Bot { get; set; }
        }

        public class IncomingWebhook
        {
            [JsonProperty("channel")]
            public string Channel { get; set; }

            [JsonProperty("channel_id")]
            public string ChannelId { get; set; }

            [JsonProperty("configuration_url")]
            public string ConfigurationUrl { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class Bot
        {
            [JsonProperty("bot_user_id")]
            public string BotUserId { get; set; }

            [JsonProperty("bot_access_token")]
            public string BotAccessToken { get; set; }
        }
    }

}
