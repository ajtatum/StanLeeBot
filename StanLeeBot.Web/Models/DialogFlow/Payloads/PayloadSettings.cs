using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public class PayloadSettings
    {
        [JsonProperty("google", NullValueHandling = NullValueHandling.Ignore)]
        public GooglePayloadSettings.Payload Google { get; set; }

        [JsonProperty("facebook", NullValueHandling = NullValueHandling.Ignore)]
        public FacebookPayloadSettings.Payload Facebook { get; set; }

        [JsonProperty("slack", NullValueHandling = NullValueHandling.Ignore)]
        public SlackPayloadSettings.Payload Slack { get; set; }

        [JsonProperty("telegram", NullValueHandling = NullValueHandling.Ignore)]
        public TelegramPayloadSettings.Payload Telegram { get; set; }
    }
}
