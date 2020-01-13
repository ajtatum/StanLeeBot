using System;
using System.Collections.Generic;
using Google.Cloud.Dialogflow.V2;
using Newtonsoft.Json;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Models.DialogFlow
{
    public class DialogFlowResponse
    {
        [JsonProperty("fulfillmentText", NullValueHandling = NullValueHandling.Ignore)]
        public string FulfillmentText { get; set; }

        [JsonProperty("fulfillmentMessages", NullValueHandling = NullValueHandling.Ignore)]
        public List<ResponseFulfillmentMessage> FulfillmentMessages { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public string Source { get; set; }

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public PayloadBuilder Payload { get; set; }

        [JsonProperty("outputContexts", NullValueHandling = NullValueHandling.Ignore)]
        public List<ResponseOutputContext> OutputContexts { get; set; }

        [JsonProperty("followupEventInput", NullValueHandling = NullValueHandling.Ignore)]
        public ResponseFollowupEventInput FollowupEventInput { get; set; }
    }

    public class ResponseFollowupEventInput
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
        public string LanguageCode { get; set; }

        [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
        public ResponseParameters Parameters { get; set; }
    }

    public class ResponseParameters
    {
        [JsonProperty("param", NullValueHandling = NullValueHandling.Ignore)]
        public string Param { get; set; }
    }

    public class ResponseFulfillmentMessage
    {
        [JsonProperty("card", NullValueHandling = NullValueHandling.Ignore)]
        public ResponseCard Card { get; set; }
    }

    public class ResponseCard
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
        public string Subtitle { get; set; }

        [JsonProperty("imageUri", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageUri { get; set; }

        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public List<ResponseButton> Buttons { get; set; }
    }

    public class ResponseButton
    {
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("postback", NullValueHandling = NullValueHandling.Ignore)]
        public string PostBack { get; set; }
    }

    public class ResponseOutputContext
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("lifespanCount", NullValueHandling = NullValueHandling.Ignore)]
        public long LifespanCount { get; set; }

        [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
        public ResponseParameters Parameters { get; set; }
    }

    public class ResponsePayload
    {
        [JsonProperty("google", NullValueHandling = NullValueHandling.Ignore)]
        public GooglePayloadSettings Google { get; set; }

        [JsonProperty("facebook", NullValueHandling = NullValueHandling.Ignore)]
        public FacebookPayloadSettings Facebook { get; set; }

        [JsonProperty("slack", NullValueHandling = NullValueHandling.Ignore)]
        public SlackPayloadSettings Slack { get; set; }

        [JsonProperty("telegram", NullValueHandling = NullValueHandling.Ignore)]
        public TelegramPayloadSettings Telegram { get; set; }
    }
}
