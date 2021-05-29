using System.Collections.Generic;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.Search.Payloads
{
    public class GooglePayload
    {
        public static GooglePayloadSettings.Payload Build(string title, string bio, string imageUrl, string webSite)
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
                                TextToSpeech = $"Excelsior! I found {title}! {bio}",
                                DisplayText = $"Excelsior! I found {title}! {bio}"
                            }
                        },
                        new GooglePayloadSettings.Item()
                        {
                            BasicCard = new GooglePayloadSettings.BasicCard()
                            {
                                Title = $"Excelsior! I found {title}!",
                                Subtitle = "From Marvel.com...",
                                FormattedText = bio,
                                Image = new GooglePayloadSettings.Image()
                                {
                                    Url = imageUrl,
                                    AccessibilityText = title
                                },
                                ImageDisplayOptions = "DEFAULT",
                                Buttons = new List<GooglePayloadSettings.Button>()
                                {
                                    new GooglePayloadSettings.Button()
                                    {
                                        Title = "Read more",
                                        OpenUrlAction = new GooglePayloadSettings.OpenUrlAction()
                                        {
                                            Url = webSite
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return payloadSettings;
        }
    }
}
