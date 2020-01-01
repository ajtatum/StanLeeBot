using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models
{
    public class SlackCommandRequest
    {
        [JsonProperty("token")]
        [FromForm(Name = "token")]
        public string Token { get; set; }

        [JsonProperty("team_id")]
        [FromForm(Name = "team_id")]
        public string TeamId { get; set; }

        [JsonProperty("team_domain")]
        [FromForm(Name = "team_domain")]
        public string TeamDomain { get; set; }

        [JsonProperty("enterprise_id")]
        [FromForm(Name = "enterprise_id")]
        public string EnterpriseId { get; set; }

        [JsonProperty("enterprise_name")]
        [FromForm(Name = "enterprise_name")]
        public string EnterpriseName { get; set; }

        [JsonProperty("channel_id")]
        [FromForm(Name = "channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("channel_name")]
        [FromForm(Name = "channel_name")]
        public string ChannelName { get; set; }

        [JsonProperty("user_id")]
        [FromForm(Name = "user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        [FromForm(Name = "user_name")]
        public string UserName { get; set; }

        [JsonProperty("command")]
        [FromForm(Name = "command")]
        public string Command { get; set; }

        [JsonProperty("text")]
        [FromForm(Name = "text")]
        public string Text { get; set; }

        [JsonProperty("response_url")]
        [FromForm(Name = "response_url")]
        public string ResponseUrl { get; set; }

        [JsonProperty("trigger_id")]
        [FromForm(Name = "trigger_id")]
        public string TriggerId { get; set; }
    }
}
