using System.Collections.Generic;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.Search.Payloads
{
    public static class DefaultPayload
    {
        public static PayloadSettings Build(string searchTerm)
        {
            var payloadBuilder = new PayloadSettings()
            {
                Google = new GooglePayloadSettings.Payload()
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
                                    TextToSpeech = $"Couldn't find anything for {searchTerm}"
                                }
                            }
                        }
                    }
                },
                Facebook = new FacebookPayloadSettings.Payload()
                {
                    Text = $"Couldn't find anything for {searchTerm}",
                }
            };

            return payloadBuilder;
        }
    }
}
