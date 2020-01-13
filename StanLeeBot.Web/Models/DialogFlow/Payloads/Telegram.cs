using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public class TelegramPayloadSettings
    {
        /// <summary>
        /// Example: You can read about *entities* [here](/docs/concept-entities).
        /// </summary>
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        /// <summary>
        /// Markdown
        /// </summary>
        [JsonProperty("parse_mode", NullValueHandling = NullValueHandling.Ignore)]
        public string ParseMode { get; set; }
    }
}
