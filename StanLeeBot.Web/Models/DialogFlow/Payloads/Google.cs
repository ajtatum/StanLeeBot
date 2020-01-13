using System;
using System.Collections.Generic;
using BabouExtensions;
using Google.Cloud.Dialogflow.V2;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models.DialogFlow.Payloads
{
    public class GooglePayloadSettings
    {
        [JsonProperty("expectUserResponse", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExpectUserResponse { get; set; }

        [JsonProperty("richResponse", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleRichResponse RichResponse { get; set; }
    }

    public class GoogleRichResponse
    {
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<GoogleItem> Items { get; set; }
    }

    public class GoogleItem
    {
        [JsonProperty("simpleResponse", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleSimpleResponse SimpleResponse { get; set; }

        [JsonProperty("basicCard", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleBasicCard BasicCard { get; set; }

        [JsonProperty("linkOutSuggestion", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleLinkOutSuggestion LinkOutSuggestion { get; set; }
    }

    public class GoogleSimpleResponse
    {
        [JsonProperty("textToSpeech", NullValueHandling = NullValueHandling.Ignore)]
        public string TextToSpeech { get; set; }

        /// <summary>
        /// Optional. If no value is set, uses TextToSpeech.
        /// </summary>
        [JsonProperty("displayText", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayText { get; set; }
    }

    public class GoogleBasicCard
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
        public string Subtitle { get; set; }

        [JsonProperty("formattedText", NullValueHandling = NullValueHandling.Ignore)]
        public string FormattedText { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleImage Image { get; set; }

        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public List<GoogleButton> Buttons { get; set; }

        /// <summary>
        /// DEFAULT, WHITE, CROPPED
        /// </summary>
        [JsonProperty("imageDisplayOptions", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageDisplayOptions { get; set; }
    }

    public class GoogleLinkOutSuggestion
    {
        [JsonProperty("destinationName", NullValueHandling = NullValueHandling.Ignore)]
        public string DestinationName { get; set; }

        [JsonProperty("openUrlAction", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleOpenUrlAction OpenUrlAction { get; set; }
    }

    public class GoogleButton
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("openUrlAction", NullValueHandling = NullValueHandling.Ignore)]
        public GoogleOpenUrlAction OpenUrlAction { get; set; }
    }

    public class GoogleOpenUrlAction
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }

    public class GoogleImage
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("accessibilityText", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessibilityText { get; set; }
    }
}
