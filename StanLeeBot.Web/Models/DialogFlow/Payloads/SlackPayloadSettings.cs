using System.Collections.Generic;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public static class SlackPayloadSettings
    {
        public class Payload
        {
            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string Text { get; set; }

            [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
            public List<SlackBotMessages.Models.Attachment> Attachments { get; set; }
        }
    }

}
