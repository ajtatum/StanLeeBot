using System.Collections.Generic;
using Newtonsoft.Json;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Models.DialogFlow
{
    public static class DialogFlowResponse
    {
        public class Response
        {
            [JsonProperty("fulfillmentText", NullValueHandling = NullValueHandling.Ignore)]
            public string FulfillmentText { get; set; }

            [JsonProperty("fulfillmentMessages", NullValueHandling = NullValueHandling.Ignore)]
            public List<FulfillmentMessage> FulfillmentMessages { get; set; }

            [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
            public string Source { get; set; }

            [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
            public PayloadSettings Payload { get; set; }

            [JsonProperty("outputContexts", NullValueHandling = NullValueHandling.Ignore)]
            public List<OutputContext> OutputContexts { get; set; }

            [JsonProperty("followupEventInput", NullValueHandling = NullValueHandling.Ignore)]
            public FollowupEventInput FollowupEventInput { get; set; }
        }

        public class FulfillmentMessage
        {
            [JsonProperty("card", NullValueHandling = NullValueHandling.Ignore)]
            public Card Card { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public Text Text { get; set; }
        }

        public class Text
        {
            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string[] Texts { get; set; }
        }

        public class Card
        {
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
            public string Subtitle { get; set; }

            [JsonProperty("imageUri", NullValueHandling = NullValueHandling.Ignore)]
            public string ImageUri { get; set; }

            [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
            public List<Button> Buttons { get; set; }
        }

        public class Button
        {
            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string Text { get; set; }

            [JsonProperty("postback", NullValueHandling = NullValueHandling.Ignore)]
            public string PostBack { get; set; }
        }

        public class OutputContext
        {
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("lifespanCount", NullValueHandling = NullValueHandling.Ignore)]
            public long LifespanCount { get; set; }

            [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
            public Parameters Parameters { get; set; }
        }

        public class FollowupEventInput
        {
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
            public string LanguageCode { get; set; }

            [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
            public Parameters Parameters { get; set; }
        }

        public class Parameters
        {
            [JsonProperty("param", NullValueHandling = NullValueHandling.Ignore)]
            public string Param { get; set; }
        }
    }

}
