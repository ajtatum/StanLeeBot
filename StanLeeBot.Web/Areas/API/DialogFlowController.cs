using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BabouExtensions;
using BabouExtensions.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StanLeeBot.Web.Builders.Search;
using StanLeeBot.Web.Builders.Search.Interfaces;
using StanLeeBot.Web.Builders.ShortenUrl.Interfaces;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Models.DialogFlow;

namespace StanLeeBot.Web.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DialogFlowController : ControllerBase
    {
        private readonly ILogger<DialogFlowController> _logger;
        private readonly AppSettings _appSettings;

        private readonly ISearchBuilder<MarvelSearchBuilder> _marvelSearchBuilder;
        private readonly ISearchBuilder<DCComicsSearchBuilder> _dcComicsSearchBuilder;
        private readonly IShortenUrlBuilder _shortenUrlBuilder;

        public DialogFlowController(ILogger<DialogFlowController> logger, IOptionsMonitor<AppSettings> appSettings,
                                    ISearchBuilder<MarvelSearchBuilder> marvelSearchBuilder, ISearchBuilder<DCComicsSearchBuilder> dcComicsSearchBuilder,
                                    IShortenUrlBuilder shortenUrlBuilder)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _marvelSearchBuilder = marvelSearchBuilder;
            _dcComicsSearchBuilder = dcComicsSearchBuilder;
            _shortenUrlBuilder = shortenUrlBuilder;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult("I'm alive.");
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            _logger.LogDebug("DialogFlow: Received Request");

            var sessionId = string.Empty;
            var intentName = string.Empty;

            try
            {
                Request.EnableBuffering();
                var requestBody = await Request.GetRawBodyStringAsyncWithOptions(null, null, true);
                var headers = Request.Headers;
                var authHeader = headers[_appSettings.DialogFlow.HeaderName].ToString();

                _logger.LogDebug("DialogFlow: Beginning Request header validation.");

                if (authHeader == _appSettings.DialogFlow.HeaderValue)
                {
                    _logger.LogDebug("DialogFlow: Request header matches!");

                    _logger.LogInformation("DialogFlow: Received WebHookRequest: {RawRequestBody}", requestBody);

                    dynamic webHookRequest = JsonConvert.DeserializeObject(requestBody);
                    //_logger.LogInformation("DialogFlow: Received WebHookRequest: {WebHookRequest}", webHookRequest);

                    sessionId = webHookRequest.session;
                    sessionId = sessionId.Substring(sessionId.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    intentName = webHookRequest.queryResult.intent.name;
                    intentName = intentName.Substring(intentName.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    string queryText = webHookRequest.queryResult.queryText;

                    string originalDetectIntentRequest;
                    try
                    {
                        originalDetectIntentRequest = webHookRequest.originalDetectIntentRequest.source;
                    }
                    catch //this is horrible
                    {
                        originalDetectIntentRequest = "DialogFlow";
                    }

                    var originSource = originalDetectIntentRequest switch
                    {
                        "facebook" => OriginSources.Facebook,
                        "slack" => OriginSources.Slack,
                        "telegram" => OriginSources.Telegram,
                        _ => OriginSources.DialogFlow
                    };

                    _logger.LogInformation("DialogFlow: Processing request for intent {Intent} and queryText {QueryText} using {OriginSource}. SessionId: {SessionId}.", intentName, queryText, originSource, sessionId);

                    var webHookResponse = new DialogFlowResponse.Response();

                    switch (intentName)
                    {
                        case Constants.DialogFlow.HelloMarvelLookupIntentId:
                            _logger.LogDebug("DialogFlow: Beginning to request Marvel Lookup. SessionId: {SessionId}.", sessionId);

                            var (marvelFulfillmentText, marvelFulfillmentMessage, marvelPayLoad) = await _marvelSearchBuilder.Build(queryText, sessionId);

                            webHookResponse.FulfillmentText = marvelFulfillmentText;
                            webHookResponse.FulfillmentMessages = new List<DialogFlowResponse.FulfillmentMessage>
                            {
                                marvelFulfillmentMessage
                            };
                            webHookResponse.Payload = marvelPayLoad;
                            break;
                        case Constants.DialogFlow.HelloDCLookupIntentId:
                            _logger.LogDebug("DialogFlow: Beginning to request DC Comics Lookup. SessionId: {SessionId}.", sessionId);

                            var (dcFulfillmentText, dcFulfillmentMessage, dcPayLoad) = await _dcComicsSearchBuilder.Build(queryText, sessionId);

                            webHookResponse.FulfillmentText = dcFulfillmentText;
                            webHookResponse.FulfillmentMessages = new List<DialogFlowResponse.FulfillmentMessage>
                            {
                                dcFulfillmentMessage
                            };
                            webHookResponse.Payload = dcPayLoad;
                            break;
                        case Constants.DialogFlow.HelloShortenUrlIntentId:
                        case Constants.DialogFlow.StartQuickShortenUrlIntentId:
                            _logger.LogDebug("DialogFlow: Beginning Shortening Url. SessionId: {SessionId}.", sessionId);

                            bool allRequiredParamsPresent = webHookRequest.queryResult.allRequiredParamsPresent;

                            //var shorteningInfo = fulfillmentText.Split(" ");
                            var longUrl = string.Empty;
                            var shortDomain = string.Empty;
                            var emailAddress = string.Empty;

                            if (allRequiredParamsPresent)
                            {
                                longUrl = webHookRequest.queryResult.parameters.LongUrl;
                                shortDomain = webHookRequest.queryResult.parameters.ShortUrlDomain;
                                emailAddress = webHookRequest.queryResult.parameters.UserEmail;
                            }

                            var (shortenFulfillmentText, shortenFulfillmentMessage, shortenPayload) = await _shortenUrlBuilder.Build(longUrl, shortDomain, emailAddress, originSource, sessionId);

                            webHookResponse.FulfillmentText = shortenFulfillmentText;
                            webHookResponse.FulfillmentMessages = new List<DialogFlowResponse.FulfillmentMessage>
                            {
                                shortenFulfillmentMessage
                            };
                            webHookResponse.Payload = shortenPayload;
                            break;
                        default:
                            _logger.LogWarning("DialogFlow: Unhandled intent: {IntentName}. SessionId: {SessionId}.", intentName, sessionId);
                            webHookResponse.FulfillmentText = "Sorry, but I don't understand.";
                            break;
                    }

                    _logger.LogInformation("DialogFlow: WebHookResponse: {@WebHookResponse}", webHookResponse);

                    return new OkObjectResult(webHookResponse);
                }
                else
                {
                    if (authHeader.IsNullOrWhiteSpace())
                        _logger.LogError("DialogFlow: Request header is absent.");
                    else
                        _logger.LogError("DialogFlow: Request header do not match. Value sent: {AuthHeader}", authHeader);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: Error during request. Intent Name: {IntentName}. SessionId: {SessionId}.", intentName, sessionId);
                return new BadRequestResult();
            }

            return new UnauthorizedResult();
        }
    }
}