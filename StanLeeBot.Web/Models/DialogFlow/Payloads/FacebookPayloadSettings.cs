using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public static class FacebookPayloadSettings
    {
        public class Payload
        {
            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string Text { get; set; }

            [JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
            public Attachment Attachment { get; set; }
        }

        public class Attachment
        {
            /// <summary>
            /// template
            /// </summary>
            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
            public AttachmentPayload Payload { get; set; }
        }

        public class AttachmentPayload
        {
            /// <summary>
            /// generic
            /// </summary>
            [JsonProperty("template_type", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateType { get; set; }

            /// <summary>
            /// horizontal (1.91:1) or square. Defaults to horizontal.
            /// </summary>
            [JsonProperty("image_aspect_ratio", NullValueHandling = NullValueHandling.Ignore)]
            public string ImageAspectRatio { get; set; }

            [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
            public List<AttachmentElement> Elements { get; set; }
        }

        public class AttachmentElement
        {
            /// <summary>
            /// The title to display in the template. 80 character limit.
            /// </summary>
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            /// <summary>
            /// Optional. The URL of the image to display in the template.
            /// </summary>
            [JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
            public string ImageUrl { get; set; }

            /// <summary>
            /// Optional. The subtitle to display in the template. 80 character limit.
            /// </summary>
            [JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
            public string Subtitle { get; set; }

            /// <summary>
            /// Optional. The default action executed when the template is tapped. Accepts the same properties as URL button, except title.
            /// </summary>
            [JsonProperty("default_action", NullValueHandling = NullValueHandling.Ignore)]
            public ElementDefaultAction DefaultAction { get; set; }

            /// <summary>
            /// Optional. An array of buttons to append to the template. A maximum of 3 buttons per element is supported.
            /// </summary>
            [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
            public List<ElementButton> Buttons { get; set; }
        }

        public class ElementButton
        {
            /// <summary>
            /// Type of button. Must be web_url.
            /// </summary>
            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            /// <summary>
            /// This URL is opened in a mobile browser when the button is tapped.
            /// </summary>
            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            /// <summary>
            /// Button title. 20 character limit.
            /// </summary>
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            /// <summary>
            /// Optional. Height of the WebView. Valid values: compact, tall, full. Defaults to full.
            /// </summary>
            [JsonProperty("webview_height_ratio", NullValueHandling = NullValueHandling.Ignore)]
            public string WebViewHeightRatio { get; set; }
        }

        public class ElementDefaultAction
        {
            /// <summary>
            /// web_url, postback
            /// </summary>
            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            /// <summary>
            /// Optional. Height of the WebView. Valid values: compact, tall, full. Defaults to full.
            /// </summary>
            [JsonProperty("webview_height_ratio", NullValueHandling = NullValueHandling.Ignore)]
            public string WebViewHeightRatio { get; set; }
        }

        public enum WebViewHeightRatioEnum
        {
            [Description("compact")]
            Compact,
            [Description("tall")]
            Tall,
            [Description("full")]
            Full
        }

        public enum ImageAspectRatioEnum
        {
            [Description("horizontal")]
            Horizontal,
            [Description("square")]
            Square
        }
    }

}
