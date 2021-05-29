using System.Collections.Generic;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public static class GooglePayloadSettings
    {
        public class Payload
        {
            [JsonProperty("expectUserResponse", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ExpectUserResponse { get; set; }

            [JsonProperty("richResponse", NullValueHandling = NullValueHandling.Ignore)]
            public RichResponse RichResponse { get; set; }
        }

        public class RichResponse
        {
            [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
            public List<Item> Items { get; set; }
        }

        public class Item
        {
            [JsonProperty("simpleResponse", NullValueHandling = NullValueHandling.Ignore)]
            public SimpleResponse SimpleResponse { get; set; }

            [JsonProperty("basicCard", NullValueHandling = NullValueHandling.Ignore)]
            public BasicCard BasicCard { get; set; }

            [JsonProperty("linkOutSuggestion", NullValueHandling = NullValueHandling.Ignore)]
            public LinkOutSuggestion LinkOutSuggestion { get; set; }
        }

        public class SimpleResponse
        {
            [JsonProperty("textToSpeech", NullValueHandling = NullValueHandling.Ignore)]
            public string TextToSpeech { get; set; }

            /// <summary>
            /// Optional. If no value is set, uses TextToSpeech.
            /// </summary>
            [JsonProperty("displayText", NullValueHandling = NullValueHandling.Ignore)]
            public string DisplayText { get; set; }
        }

        public class BasicCard
        {
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
            public string Subtitle { get; set; }

            [JsonProperty("formattedText", NullValueHandling = NullValueHandling.Ignore)]
            public string FormattedText { get; set; }

            [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
            public Image Image { get; set; }

            [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
            public List<Button> Buttons { get; set; }

            /// <summary>
            /// DEFAULT, WHITE, CROPPED
            /// </summary>
            [JsonProperty("imageDisplayOptions", NullValueHandling = NullValueHandling.Ignore)]
            public string ImageDisplayOptions { get; set; }
        }

        public class LinkOutSuggestion
        {
            [JsonProperty("destinationName", NullValueHandling = NullValueHandling.Ignore)]
            public string DestinationName { get; set; }

            [JsonProperty("openUrlAction", NullValueHandling = NullValueHandling.Ignore)]
            public OpenUrlAction OpenUrlAction { get; set; }
        }

        public class Button
        {
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty("openUrlAction", NullValueHandling = NullValueHandling.Ignore)]
            public OpenUrlAction OpenUrlAction { get; set; }
        }

        public class OpenUrlAction
        {
            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }
        }

        public class Image
        {
            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            [JsonProperty("accessibilityText", NullValueHandling = NullValueHandling.Ignore)]
            public string AccessibilityText { get; set; }
        }
    }


}
