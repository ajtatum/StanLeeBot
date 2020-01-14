using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions;
using BabouExtensions.AspNetCore;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StanLeeBot.Web.Helpers;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DialogFlowController : ControllerBase
    {
        private readonly ILogger<DialogFlowController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IGoogleCustomSearch _googleCustomSearch;

        public DialogFlowController(ILogger<DialogFlowController> logger, IOptionsMonitor<AppSettings> appSettings, IGoogleCustomSearch googleCustomSearch)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _googleCustomSearch = googleCustomSearch;
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

                    var webHookRequest = JsonConvert.DeserializeObject<WebhookRequest>(requestBody);
                    _logger.LogInformation("DialogFlow: Received WebHookRequest: {WebHookRequest}", webHookRequest);

                    sessionId = webHookRequest.Session;
                    sessionId = sessionId.Substring(sessionId.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    intentName = webHookRequest.QueryResult.Intent.Name;
                    intentName = intentName.Substring(intentName.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    var queryText = webHookRequest.QueryResult.QueryText;

                    _logger.LogInformation("DialogFlow: Processing request for intent {Intent} and queryText {QueryText}. SessionId: {SessionId}.", intentName, queryText, sessionId);

                    var webHookResponse = new DialogFlowResponse();

                    switch (intentName)
                    {
                        case Constants.DialogFlow.HelloMarvelLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request Marvel Lookup. SessionId: {SessionId}.", sessionId);

                            var (marvelFulfillmentText, marvelFulfillmentMessage, marvelPayLoad) = await GetMarvelBuilder(queryText, sessionId);

                            webHookResponse.FulfillmentText = marvelFulfillmentText;
                            webHookResponse.FulfillmentMessages = new List<ResponseFulfillmentMessage>
                            {
                                marvelFulfillmentMessage
                            };
                            webHookResponse.Payload = marvelPayLoad;
                            break;
                        case Constants.DialogFlow.HelloDCLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request DC Comics Lookup. SessionId: {SessionId}.", sessionId);

                            var (dcFulfillmentText, dcFulfillmentMessage, dcPayLoad) = await GetDcComicsBuilder(queryText, sessionId);

                            webHookResponse.FulfillmentText = dcFulfillmentText;
                            webHookResponse.FulfillmentMessages = new List<ResponseFulfillmentMessage>
                            {
                                dcFulfillmentMessage
                            };
                            webHookResponse.Payload = dcPayLoad;
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

        private async Task<(string, ResponseFulfillmentMessage, PayloadBuilder)> GetMarvelBuilder(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetMarvelBuilder - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/MarvelCard.png";

            #region Set Defaults
            var responseFulfillmentMessage = new ResponseFulfillmentMessage
            {
                Card = new ResponseCard()
                {
                    Title = "Sorry",
                    Subtitle = $"Couldn't find anything for {searchTerm}",
                    ImageUri = backupImage,
                    Buttons = new List<ResponseButton>()
                }
            };

            var responseFulfillmentText = $"Couldn't find anything for {searchTerm}";

            var payloadBuilder = BuildDefaultPayLoad(searchTerm);
            #endregion

            try
            {
                var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
                var marvelGsr = await _googleCustomSearch.GetResponse(searchTerm, marvelGoogleCx);

                if (marvelGsr.Items != null)
                {
                    var firstGsrItem = marvelGsr.Items.ElementAt(0);
                    var gsrMetaTags = firstGsrItem.PageMap.MetaTags.ElementAtOrDefault(0);

                    if (gsrMetaTags != null)
                    {
                        _logger.LogInformation("DialogFlow: GetMarvelBuilder - Beginning to build payload for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        var title = CleanTitle(gsrMetaTags.OgTitle) ?? CleanTitle(firstGsrItem.Title) ?? searchTerm;
                        var bio = gsrMetaTags.OgDescription.ToNullIfWhiteSpace() ?? firstGsrItem.Snippet.CleanString().ToNullIfWhiteSpace() ?? "Description unavailable at this time.";
                        var imageUrl = firstGsrItem.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                        var webSite = gsrMetaTags.OgUrl.ToNullIfWhiteSpace() ?? firstGsrItem.Link.ToNullIfWhiteSpace() ?? "https://www.marvel.com/";

                        var responseCard = new ResponseCard
                        {
                            Title = title, 
                            Subtitle = bio, 
                            ImageUri = imageUrl,
                            Buttons = new List<ResponseButton>()
                            {
                                new ResponseButton()
                                {
                                    PostBack = webSite,
                                    Text = "Excelsior! Read more..."
                                }
                            }
                        };

                        responseFulfillmentText = $"Excelsior! I found {title}! {bio}";
                        responseFulfillmentMessage.Card = responseCard;

                        payloadBuilder.Google = BuildGooglePayload(title, bio, imageUrl, webSite);
                        payloadBuilder.Facebook = BuildFacebookPayload(title, bio, imageUrl, webSite, FacebookImageAspectRatio.Horizontal, FacebookWebViewHeightRatio.Full);

                        _logger.LogInformation("DialogFlow: GetMarvelBuilder - Payload built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        _logger.LogDebug("DialogFlow: GetMarvelBuilder - PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);
                        return (responseFulfillmentText, responseFulfillmentMessage, payloadBuilder);
                    }

                    _logger.LogError("DialogFlow: GetMarvelBuilder - Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetMarvelBuilder - There are no items for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetMarvelBuilder - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("DialogFlow: GetMarvelBuilder - Default PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return (responseFulfillmentText, responseFulfillmentMessage, payloadBuilder);

            static string CleanTitle(string title)
            {
                return title.IsNullOrWhiteSpace()
                    ? null
                    : title.Split('|').ElementAtOrDefault(0)?.Trim().ToNullIfWhiteSpace();
            }
        }

        private async Task<(string, ResponseFulfillmentMessage, PayloadBuilder)> GetDcComicsBuilder(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetDcComicsBuilder - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/DCCard.png";

            #region Set Defaults
            var responseFulfillmentMessage = new ResponseFulfillmentMessage
            {
                Card = new ResponseCard()
                {
                    Title = "Sorry",
                    Subtitle = $"Couldn't find anything for {searchTerm}",
                    ImageUri = backupImage,
                    Buttons = new List<ResponseButton>()
                }
            };

            var responseFulfillmentText = $"Couldn't find anything for {searchTerm}";

            var payloadBuilder = BuildDefaultPayLoad(searchTerm);
            #endregion

            try
            {
                var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
                var dcGsr = await _googleCustomSearch.GetResponse(searchTerm, dcComicsCx);

                if (dcGsr.Items != null)
                {
                    var firstGsrItem = dcGsr.Items.ElementAt(0);
                    var gsrMetaTags = firstGsrItem.PageMap.MetaTags.ElementAtOrDefault(0);

                    if (gsrMetaTags != null)
                    {
                        _logger.LogInformation("DialogFlow: GetDcComicsBuilder - Beginning to build payload for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        var title = gsrMetaTags.OgTitle.ToNullIfWhiteSpace() ?? firstGsrItem.Title.ToNullIfWhiteSpace() ?? searchTerm;
                        var bio = gsrMetaTags.OgDescription.ToNullIfWhiteSpace() ?? firstGsrItem.Snippet.CleanString().ToNullIfWhiteSpace() ?? "Description unavailable at this time.";
                        var imageUrl = firstGsrItem.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                        var webSite = gsrMetaTags.OgUrl.ToNullIfWhiteSpace() ?? firstGsrItem.Link.ToNullIfWhiteSpace() ?? "https://www.dccomics.com/";

                        var responseCard = new ResponseCard
                        {
                            Title = title, 
                            Subtitle = bio, 
                            ImageUri = imageUrl,
                            Buttons = new List<ResponseButton>()
                            {
                                new ResponseButton()
                                {
                                    PostBack = webSite,
                                    Text = "Excelsior! Read more..."
                                }
                            }
                        };

                        responseFulfillmentText = $"Excelsior! I found {title}! {bio}";
                        responseFulfillmentMessage.Card = responseCard;

                        payloadBuilder.Google = BuildGooglePayload(title, bio, imageUrl, webSite);
                        payloadBuilder.Facebook = BuildFacebookPayload(title, bio, imageUrl, webSite, FacebookImageAspectRatio.Square, FacebookWebViewHeightRatio.Tall);

                        _logger.LogInformation("DialogFlow: GetDcComicsBuilder - Payload built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        _logger.LogDebug("DialogFlow: GetDcComicsBuilder - PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);
                        return (responseFulfillmentText, responseFulfillmentMessage, payloadBuilder);
                    }

                    _logger.LogError("DialogFlow: GetDcComicsBuilder - Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetDcComicsBuilder - There are no items for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetDcComicsBuilder - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("DialogFlow: GetDcComicsBuilder - Default PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return (responseFulfillmentText, responseFulfillmentMessage, payloadBuilder);
        }

        private static PayloadBuilder BuildDefaultPayLoad(string searchTerm)
        {
            var payloadBuilder = new PayloadBuilder()
            {
                Google = new GooglePayloadSettings
                {
                    ExpectUserResponse = false,
                    RichResponse = new GoogleRichResponse()
                    {
                        Items = new List<GoogleItem>()
                        {
                            new GoogleItem()
                            {
                                SimpleResponse = new GoogleSimpleResponse()
                                {
                                    TextToSpeech = $"Couldn't find anything for {searchTerm}"
                                }
                            }
                        }
                    }
                },
                Facebook = new FacebookPayloadSettings()
                {
                    Text = $"Couldn't find anything for {searchTerm}",
                }
            };

            return payloadBuilder;
        }

        private static GooglePayloadSettings BuildGooglePayload(string title, string bio, string imageUrl, string webSite)
        {
            var payload = new GooglePayloadSettings()
            {
                ExpectUserResponse = false,
                RichResponse = new GoogleRichResponse()
                {
                    Items = new List<GoogleItem>()
                    {
                        new GoogleItem()
                        {
                            SimpleResponse = new GoogleSimpleResponse()
                            {
                                TextToSpeech = $"Excelsior! I found {title}! {bio}",
                                DisplayText = $"Excelsior! I found {title}! {bio}"
                            }
                        },
                        new GoogleItem()
                        {
                            BasicCard = new GoogleBasicCard()
                            {
                                Title = $"Excelsior! I found {title}!",
                                Subtitle = "From Marvel.com...",
                                FormattedText = bio,
                                Image = new GoogleImage()
                                {
                                    Url = imageUrl,
                                    AccessibilityText = title
                                },
                                ImageDisplayOptions = "DEFAULT",
                                Buttons = new List<GoogleButton>()
                                {
                                    new GoogleButton()
                                    {
                                        Title = "Read more",
                                        OpenUrlAction = new GoogleOpenUrlAction()
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

            return payload;
        }

        private static FacebookPayloadSettings BuildFacebookPayload(string title, string bio, string imageUrl, string webSite, FacebookImageAspectRatio imageAspectRatio, FacebookWebViewHeightRatio viewHeightRatio)
        {
            var payLoad = new FacebookPayloadSettings
            {
                Text = null,
                Attachment = new FacebookAttachment()
                {
                    Type = "template",
                    Payload = new FacebookPayload()
                    {
                        TemplateType = "generic",
                        ImageAspectRatio = imageAspectRatio.GetDescriptionAttr(),
                        Elements = new List<FacebookElement>()
                        {
                            new FacebookElement
                            {
                                Buttons = new List<FacebookButton>()
                                {
                                    new FacebookButton()
                                    {
                                        Title = "Excelsior! Read more...", 
                                        Type = "web_url", Url = webSite, 
                                        WebViewHeightRatio = viewHeightRatio.GetDescriptionAttr()
                                    }
                                },
                                Title = title,
                                Subtitle = bio,
                                ImageUrl = imageUrl,
                                DefaultAction = new FacebookDefaultAction()
                                {
                                    Type = "web_url", 
                                    Url = webSite, 
                                    WebViewHeightRatio = viewHeightRatio.GetDescriptionAttr()
                                }
                            }
                        }
                    }
                }
            };

            return payLoad;
        }
    }
}