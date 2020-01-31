using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StanLeeBot.Web.Builders.Search.Interfaces;
using StanLeeBot.Web.Builders.ShortenUrl.Interfaces;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.ShortenUrl
{
    public class ShortenUrlBuilder : IShortenUrlBuilder
    {
        private readonly ILogger<ShortenUrlBuilder> _logger;
        private readonly AppSettings _appSettings;

        public ShortenUrlBuilder(ILogger<ShortenUrlBuilder> logger, IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
        }

        public async Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string longUrl, string domain, string sessionId)
        {
            _logger.LogInformation("ShortenUrlBuilder: Received request to shorten {LongUrl} to a {Domain} domain. SessionId: {SessionId}.", longUrl, domain, sessionId);

            #region Set Defaults
            var fulfillmentMessage = new DialogFlowResponse.FulfillmentMessage
            {
                Card = new DialogFlowResponse.Card()
                {
                    Title = "Sorry",
                    Subtitle = $"Sorry, couldn't shorten {longUrl} to a {domain} domain.",
                    Buttons = new List<DialogFlowResponse.Button>()
                }
            };

            var fulfillmentText = $"Sorry, couldn't shorten {longUrl} to a {domain} domain.";

            var payloadBuilder = ShortenUrl.Payloads.DefaultPayload.Build(longUrl,domain);
            #endregion

            return (fulfillmentText, fulfillmentMessage, payloadBuilder);
        }
    }
}
