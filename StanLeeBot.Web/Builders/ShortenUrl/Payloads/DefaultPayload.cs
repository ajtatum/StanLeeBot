using System.Collections.Generic;
using StanLeeBot.Web.Models.DialogFlow.Payloads;


namespace StanLeeBot.Web.Builders.ShortenUrl.Payloads
{
    public static class DefaultPayload
    {
        public static PayloadSettings Build(string longUrl, string domain)
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
                                    TextToSpeech = $"Sorry, couldn't shorten {longUrl} to a {domain} domain."
                                }
                            }
                        }
                    }
                },
                Facebook = new FacebookPayloadSettings.Payload()
                {
                    Text = $"Sorry, couldn't shorten {longUrl} to a {domain} domain.",
                }
            };

            return payloadBuilder;
        }
    }
}
