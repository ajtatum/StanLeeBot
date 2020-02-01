using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using StanLeeBot.Web.Builders.ShortenUrl.Interfaces;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web.Builders.ShortenUrl
{
    public class ShortenUrlBuilder : IShortenUrlBuilder
    {
        private readonly ILogger<ShortenUrlBuilder> _logger;
        private readonly IUrlShorteningService _urlShorteningService;

        public ShortenUrlBuilder(ILogger<ShortenUrlBuilder> logger, IUrlShorteningService urlShorteningService)
        {
            _logger = logger;
            _urlShorteningService = urlShorteningService;
        }

        public async Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string longUrl, string domain, string emailAddress, UrlShorteningServices originSource, string sessionId)
        {
            _logger.LogInformation("ShortenUrlBuilder: Received request to shorten {LongUrl} to a {Domain} domain from {OriginSource}. SessionId: {SessionId}.", longUrl, domain, originSource, sessionId);

            #region Set Defaults
            //var responseFulfillmentMessage = new DialogFlowResponse.FulfillmentMessage
            //{
            //    Text = $"Sorry, couldn't shorten {longUrl} to a {domain} domain."
            //};

            var responseFulfillmentText = $"Sorry, couldn't shorten {longUrl} to a {domain} domain.";

            var payloadBuilder = Payloads.DefaultPayload.Build(longUrl, domain);
            #endregion

            try
            {
                var shortenerMessage = await _urlShorteningService.Shorten(longUrl, domain, emailAddress, originSource, sessionId);

                responseFulfillmentText = shortenerMessage;
                //responseFulfillmentMessage.Text = responseFulfillmentText;

                payloadBuilder.Google = Payloads.GooglePayload.Build(responseFulfillmentText.RemoveLineEndings());
                payloadBuilder.Facebook = Payloads.FacebookPayload.Build(responseFulfillmentText);
            }
            catch (Exception ex)
            {
                _logger.LogError("ShortenUrlBuilder: There was an error while trying to use the UrlShorteningService. Error: {@Error}", ex);
            }

            return (responseFulfillmentText, null, payloadBuilder);
        }
    }
}
