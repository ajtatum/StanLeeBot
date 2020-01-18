using System.Collections.Generic;
using BabouExtensions;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.Search.Payloads
{
    public class FacebookPayload
    {
        public static FacebookPayloadSettings.Payload Build(string title, string bio, string imageUrl, string webSite, FacebookPayloadSettings.ImageAspectRatioEnum imageAspectRatioEnum, FacebookPayloadSettings.WebViewHeightRatioEnum viewHeightRatioEnum)
        {
            var payloadSettings = new FacebookPayloadSettings.Payload
            {
                Text = null,
                Attachment = new FacebookPayloadSettings.Attachment()
                {
                    Type = "template",
                    Payload = new FacebookPayloadSettings.AttachmentPayload()
                    {
                        TemplateType = "generic",
                        ImageAspectRatio = imageAspectRatioEnum.GetDescriptionAttr(),
                        Elements = new List<FacebookPayloadSettings.AttachmentElement>()
                        {
                            new FacebookPayloadSettings.AttachmentElement
                            {
                                Buttons = new List<FacebookPayloadSettings.ElementButton>()
                                {
                                    new FacebookPayloadSettings.ElementButton()
                                    {
                                        Title = "Excelsior! Read more...",
                                        Type = "web_url", Url = webSite,
                                        WebViewHeightRatio = viewHeightRatioEnum.GetDescriptionAttr()
                                    }
                                },
                                Title = title,
                                Subtitle = bio,
                                ImageUrl = imageUrl,
                                DefaultAction = new FacebookPayloadSettings.ElementDefaultAction()
                                {
                                    Type = "web_url",
                                    Url = webSite,
                                    WebViewHeightRatio = viewHeightRatioEnum.GetDescriptionAttr()
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
