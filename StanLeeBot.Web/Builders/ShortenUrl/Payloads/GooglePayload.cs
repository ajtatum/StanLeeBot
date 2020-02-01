using System.Collections.Generic;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.ShortenUrl.Payloads
{
    public class GooglePayload
    {
        public static GooglePayloadSettings.Payload Build(string messageText)
        {
            var payloadSettings = new GooglePayloadSettings.Payload()
            {
                ExpectUserResponse = false,
                RichResponse = new GooglePayloadSettings.RichResponse()
                {
                    Items = new List<GooglePayloadSettings.Item>()
                    {
                        new GooglePayloadSettings.Item()
                        {
                            SimpleResponse = new GooglePayloadSettings.SimpleResponse()
                            {
                                TextToSpeech = messageText,
                                DisplayText = messageText
                            }
                        }
                    }
                }
            };

            return payloadSettings;
        }
    }
}
