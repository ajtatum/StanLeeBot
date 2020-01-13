using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public class PayloadBuilder
    {
        [JsonProperty("google", NullValueHandling = NullValueHandling.Ignore)]
        public GooglePayloadSettings Google { get; set; }

        [JsonProperty("facebook", NullValueHandling = NullValueHandling.Ignore)]
        public FacebookPayloadSettings Facebook { get; set; }

        [JsonProperty("slack", NullValueHandling = NullValueHandling.Ignore)]
        public SlackPayloadSettings Slack { get; set; }

        [JsonProperty("telegram", NullValueHandling = NullValueHandling.Ignore)]
        public TelegramPayloadSettings Telegram { get; set; }
    }
}
