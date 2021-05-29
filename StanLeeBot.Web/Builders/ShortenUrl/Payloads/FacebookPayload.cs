using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.ShortenUrl.Payloads
{
    public class FacebookPayload
    {
        public static FacebookPayloadSettings.Payload Build(string messageText)
        {
            var payloadSettings = new FacebookPayloadSettings.Payload
            {
                Text = messageText,
                Attachment = null
            };

            return payloadSettings;
        }
    }
}
