using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace StanLeeSlackBot.Web.Models
{
    public class SlackEventsRequest
    {
        [JsonProperty("token")]
        [FromForm(Name = "token")]
        public string Token { get; set; }
        [JsonProperty("challenge")]
        [FromForm(Name = "challenge")]
        public string Challenge { get; set; }
        [JsonProperty("type")]
        [FromForm(Name = "type")]
        public string Type { get; set; }
    }
}
