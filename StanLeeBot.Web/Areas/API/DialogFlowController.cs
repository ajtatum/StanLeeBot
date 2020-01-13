using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions;
using BabouExtensions.AspNetCore;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.Collections;
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

                            var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
                            var marvelGsr = await _googleCustomSearch.GetResponse(queryText, marvelGoogleCx);
                            
                            var (fulFillmentText, fullFillmentMessage, payLoad) = GetMarvelBuilder(marvelGsr, queryText, sessionId);

                            webHookResponse.FulfillmentText = fulFillmentText;
                            webHookResponse.FulfillmentMessages = new List<ResponseFulfillmentMessage>
                            {
                                fullFillmentMessage
                            };
                            webHookResponse.Payload = payLoad;
                            break;
                        case Constants.DialogFlow.HelloDCLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request DC Comics Lookup. SessionId: {SessionId}.", sessionId);

                            var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
                            var dcGsr = await _googleCustomSearch.GetResponse(queryText, dcComicsCx);

                            webHookResponse.Payload = GetDcComicsPayload(dcGsr, queryText, sessionId);
                            break;
                        default:
                            _logger.LogWarning("DialogFlow: Unhandled intent: {IntentName}. SessionId: {SessionId}.", intentName, sessionId);
                            webHookResponse.FulfillmentText = "Sorry, but I don't understand.";
                            break;
                    }

                    _logger.LogDebug("DialogFlow: {WebHookResponse}", JsonConvert.SerializeObject(webHookResponse));

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

        private (string, ResponseFulfillmentMessage, PayloadBuilder) GetMarvelBuilder(GoogleSearchResponse gsr, string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetMarvelBuilder - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/MarvelCard.png";

            //Set Defaults
            var payloadBuilder = new PayloadBuilder
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

            var responseFulFillmentText = $"Couldn't find anything for {searchTerm}";

            try
            {
                if (gsr.Items != null)
                {
                    var firstGsrItem = gsr.Items.ElementAt(0);
                    var gsrMetaTags = firstGsrItem.PageMap.MetaTags.ElementAtOrDefault(0);

                    if (gsrMetaTags != null)
                    {
                        _logger.LogDebug("DialogFlow: GetMarvelBuilder - Beginning to build payload for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        var defaultResponseCard = new ResponseCard()
                        {
                            Buttons = new List<ResponseButton>()
                        };

                        var facebookElement = new FacebookElement
                        {
                            Buttons = new List<FacebookButton>()
                        };

                        var title = CleanTitle(gsrMetaTags.OgTitle) ?? CleanTitle(firstGsrItem?.Title) ?? searchTerm;
                        facebookElement.Title = title;
                        defaultResponseCard.Title = title;

                        var bio = (!gsrMetaTags.OgDescription.IsNullOrWhiteSpace() ? gsrMetaTags.OgDescription : firstGsrItem?.Snippet.CleanString()) ?? "Description unavailable at this time.";
                        facebookElement.Subtitle = bio;
                        defaultResponseCard.Subtitle = bio;

                        var imageUrl = firstGsrItem?.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                        facebookElement.ImageUrl = imageUrl;
                        defaultResponseCard.ImageUri = imageUrl;

                        var webSite = (!gsrMetaTags.OgUrl.IsNullOrWhiteSpace() ? gsrMetaTags.OgUrl : firstGsrItem?.Link.ToNullIfEmpty()) ?? "https://www.marvel.com/";

                        facebookElement.DefaultAction = new FacebookDefaultAction()
                        {
                            Type = "web_url",
                            Url = webSite,
                            WebViewHeightRatio = "full"
                        };

                        facebookElement.Buttons.Add(new FacebookButton()
                        {
                            Title = "Excelsior! Read more...",
                            Type = "web_url",
                            Url = webSite
                        });

                        defaultResponseCard.Buttons.Add(new ResponseButton()
                        {
                            PostBack = webSite,
                            Text = "Excelsior! Read more..."
                        });

                        var textResponse = $"Excelsior! I found {title}! {bio}";

                        var successGoogleItem = new GoogleItem()
                        {
                            SimpleResponse = new GoogleSimpleResponse()
                            {
                                TextToSpeech = textResponse,
                                DisplayText = textResponse
                            }
                        };

                        var successGoogleCard = new GoogleItem()
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
                        };

                        payloadBuilder.Google.RichResponse.Items.Clear();
                        payloadBuilder.Google.RichResponse.Items.Add(successGoogleItem);
                        payloadBuilder.Google.RichResponse.Items.Add(successGoogleCard);

                        payloadBuilder.Facebook.Text = null;
                        payloadBuilder.Facebook.Attachment = new FacebookAttachment()
                        {
                            Type = "template",
                            Payload = new FacebookPayload()
                            {
                                TemplateType = "generic",
                                ImageAspectRatio = "horizontal",
                                Elements = new List<FacebookElement>()
                            }
                        };

                        payloadBuilder.Facebook.Attachment.Payload.Elements.Add(facebookElement);

                        responseFulFillmentText = textResponse;
                        responseFulfillmentMessage.Card = defaultResponseCard;

                        _logger.LogDebug("DialogFlow: GetMarvelBuilder - Payload built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        _logger.LogDebug("DialogFlow: GetMarvelBuilder - PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);
                        return (responseFulFillmentText, responseFulfillmentMessage, payloadBuilder);
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

            return (responseFulFillmentText, responseFulfillmentMessage, payloadBuilder);

            static string CleanTitle(string title)
            {
                return title.IsNullOrWhiteSpace()
                    ? null
                    : title.Remove(title.IndexOf('|')).Trim().ToNullIfEmpty();
            }
        }

        private PayloadBuilder GetDcComicsPayload(GoogleSearchResponse gsr, string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetDCComicsPayload - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/DCCard.png";

            //Set Defaults
            var payloadBuilder = new PayloadBuilder
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

            try
            {
                if (gsr.Items != null)
                {
                    var firstGsrItem = gsr.Items.ElementAt(0);
                    var gsrMetaTags = firstGsrItem.PageMap.MetaTags.ElementAtOrDefault(0);

                    if (gsrMetaTags != null)
                    {
                        _logger.LogDebug("DialogFlow: GetDCComicsPayload - Beginning to build payload for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        var facebookElement = new FacebookElement
                        {
                            Buttons = new List<FacebookButton>()
                        };

                        var title = gsrMetaTags.OgTitle ?? searchTerm;
                        facebookElement.Title = title;

                        var bio = (!gsrMetaTags.OgDescription.IsNullOrWhiteSpace() ? gsrMetaTags.OgDescription : firstGsrItem?.Snippet.CleanString()) ?? "Description unavailable at this time.";
                        facebookElement.Subtitle = bio;

                        var imageUrl = firstGsrItem?.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                        facebookElement.ImageUrl = imageUrl;

                        var webSite = (!gsrMetaTags.OgUrl.IsNullOrWhiteSpace() ? gsrMetaTags.OgUrl : firstGsrItem?.Link.ToNullIfEmpty()) ?? "https://www.dccomics.com/";

                        facebookElement.DefaultAction = new FacebookDefaultAction()
                        {
                            Type = "web_url",
                            Url = webSite,
                            WebViewHeightRatio = "tall"
                        };

                        facebookElement.Buttons.Add(new FacebookButton()
                        {
                            Title = "Excelsior! Read more...",
                            Type = "web_url",
                            Url = webSite,
                            WebViewHeightRatio = "tall"
                        });

                        var successGoogleItem = new GoogleItem()
                        {
                            SimpleResponse = new GoogleSimpleResponse()
                            {
                                TextToSpeech = $"Excelsior! I found {title}! {bio}",
                                DisplayText = $"Excelsior! I found {title}! {bio}."
                            }
                        };

                        var successGoogleCard = new GoogleItem()
                        {
                            BasicCard = new GoogleBasicCard()
                            {
                                Title = $"Excelsior! I found {title}!",
                                Subtitle = "From DCComics.com...",
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
                        };

                        payloadBuilder.Google.RichResponse.Items.Clear();
                        payloadBuilder.Google.RichResponse.Items.Add(successGoogleItem);
                        payloadBuilder.Google.RichResponse.Items.Add(successGoogleCard);

                        payloadBuilder.Facebook.Text = null;
                        payloadBuilder.Facebook.Attachment = new FacebookAttachment()
                        {
                            Type = "template",
                            Payload = new FacebookPayload()
                            {
                                TemplateType = "generic",
                                ImageAspectRatio = "horizontal",
                                Elements = new List<FacebookElement>()
                            }
                        };

                        payloadBuilder.Facebook.Attachment.Payload.Elements.Add(facebookElement);

                        _logger.LogDebug("DialogFlow: GetDCComicsPayload - Payload built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        _logger.LogDebug("DialogFlow: GetDCComicsPayload - PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);
                        return payloadBuilder;
                    }

                    _logger.LogError("DialogFlow: GetDCComicsPayload - Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetDCComicsPayload - There are no items for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetDCComicsPayload - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("DialogFlow: GetDCComicsPayload - Default PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return payloadBuilder;
        }

        private ResponseFulfillmentMessage GetMarvelCard(GoogleSearchResponse gsr, string searchTerm, string sessionId)
        {
            var responseFulfillmentMessage = new ResponseFulfillmentMessage();

            _logger.LogInformation("DialogFlow: GetMarvelCard - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/Messenger/MarvelCard.png";

            var card = new ResponseCard()
            {
                Title = "Sorry",
                Subtitle = $"Couldn't find anything for {searchTerm}",
                ImageUri = backupImage
            };

            try
            {
                var gsrMetaTags = gsr?.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0);

                if (gsrMetaTags != null)
                {
                    _logger.LogDebug("DialogFlow: GetMarvelCard - Beginning to build card for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                    var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim() ?? searchTerm;
                    card.Title = title;

                    var bio = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
                    card.Subtitle = bio;

                    card.ImageUri = gsr.Items.ElementAtOrDefault(0)?.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;

                    card.Buttons = new List<ResponseButton>
                    {
                        new ResponseButton()
                        {
                            PostBack = gsrMetaTags.OgUrl, 
                            Text = "Excelsior! Read more..."
                        }
                    };

                    _logger.LogDebug("DialogFlow: GetMarvelCard - Card built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetMarvelCard - Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetMarvelCard - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            responseFulfillmentMessage.Card = card;

            return responseFulfillmentMessage;
        }

        private async Task<RepeatedField<Intent.Types.Message>> GetDcComicsCard(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetDcComicsCard - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/Messenger/DCCard.png";

            var dcCard = new Intent.Types.Message.Types.Card
            {
                Title = "Sorry",
                Subtitle = $"Couldn't find anything for {searchTerm}",
                ImageUri = backupImage
            };

            try
            {
                var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
                var gsr = await _googleCustomSearch.GetResponse(searchTerm, dcComicsCx);

                var gsrMetaTags = gsr?.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0);

                if (gsrMetaTags != null)
                {
                    _logger.LogDebug("DialogFlow: GetDcComicsCard - Beginning to build card for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                    var title = gsrMetaTags.OgTitle ?? searchTerm;
                    dcCard.Title = title;

                    var bio = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
                    dcCard.Subtitle = bio;

                    dcCard.ImageUri = gsr.Items.ElementAtOrDefault(0)?.PageMap.CseThumbnail.ElementAtOrDefault(0)?.Src ?? backupImage;

                    dcCard.Buttons.Add(new Intent.Types.Message.Types.Card.Types.Button()
                    {
                        Postback = gsrMetaTags.OgUrl,
                        Text = "Excelsior! Read more..."
                    });

                    _logger.LogDebug("DialogFlow: GetDcComicsCard - Card built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetDcComicsCard - Metatags are null for {SearchTerm}.  SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetDcComicsCard - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            var messages = new RepeatedField<Intent.Types.Message>
            {
                new Intent.Types.Message()
                {
                    Card = dcCard
                }
            };

            return messages;
        }

        // This is only temporary, copying from Telegram
        private async Task<string> GetMarvel(string lookingFor)
        {
            var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
            var gsr = await _googleCustomSearch.GetResponse(lookingFor, marvelGoogleCx);

            var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new MetaTag();

            var messageBuilder = new StringBuilder();
            var snippet = $"Could not find anything for {lookingFor}.";

            if (gsr != null)
            {
                var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim() ?? lookingFor;

                var bio = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;

                if (!bio.IsNullOrWhiteSpace())
                {
                    messageBuilder.AppendLine($"Excelsior! I found {title}!");
                    messageBuilder.AppendLine(bio);
                }

                if (!gsrMetaTags.OgUrl.IsNullOrWhiteSpace())
                {
                    messageBuilder.AppendLine($"See more at {gsrMetaTags.OgUrl}.");
                }

                snippet = messageBuilder.ToString();
            }

            return snippet;
        }

        private async Task<string> GetDcComics(string lookingFor)
        {
            var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
            var gsr = await _googleCustomSearch.GetResponse(lookingFor, dcComicsCx);

            var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new MetaTag();

            var messageBuilder = new StringBuilder();
            var snippet = $"Could not find anything for {lookingFor}.";

            if (gsr != null)
            {
                var title = gsrMetaTags.OgTitle ?? lookingFor;

                var bio = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;

                if (!bio.IsNullOrWhiteSpace())
                {
                    messageBuilder.AppendLine($"Excelsior! I found {title}!");
                    messageBuilder.AppendLine(bio);
                }

                if (!gsrMetaTags.OgUrl.IsNullOrWhiteSpace())
                {
                    messageBuilder.AppendLine($"See more at {gsrMetaTags.OgUrl}.");
                }

                snippet = messageBuilder.ToString();
            }

            return snippet;
        }
    }
}