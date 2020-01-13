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
                    
                    var webhookRequest = JsonConvert.DeserializeObject<WebhookRequest>(requestBody);
                    _logger.LogInformation("DialogFlow: Received WebHookRequest: {WebHookRequest}", webhookRequest);

                    sessionId = webhookRequest.Session;
                    sessionId = sessionId.Substring(sessionId.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    intentName = webhookRequest.QueryResult.Intent.Name;
                    intentName = intentName.Substring(intentName.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    var queryText = webhookRequest.QueryResult.QueryText;

                    _logger.LogInformation("DialogFlow: Processing request for intent {Intent} and queryText {QueryText}. SessionId: {SessionId}.", intentName, queryText, sessionId);

                    var webHookResponse = new DialogFlowResponse();
                    
                    //TODO: I want to provide a default FulfillmentMessage for DialogFlow to show a response. So, lets get the GCS first and pass it into a payload method and a fulfillment message method.
                    switch (intentName)
                    {
                        case Constants.DialogFlow.HelloMarvelLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request Marvel Lookup. SessionId: {SessionId}.", sessionId);
                            //webHookResponse.FulfillmentMessages.Add(await GetMarvelCard(queryText, sessionId));
                            webHookResponse.Payload = await GetMarvelPayload(queryText, sessionId);
                            break;
                        case Constants.DialogFlow.HelloDCLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request DC Comics Lookup. SessionId: {SessionId}.", sessionId);
                            webHookResponse.Payload = await GetDCComicsPayload(queryText, sessionId);
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

        private async Task<PayloadBuilder> GetMarvelPayload(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetMarvelPayload - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/MarvelCard.png";

            var errorGoogleItem = new GoogleItem()
            {
                SimpleResponse = new GoogleSimpleResponse()
                {
                    TextToSpeech = $"Couldn't find anything for {searchTerm}"
                }
            };

            var payloadBuilder = new PayloadBuilder
            {
                Google = new GooglePayloadSettings
                {
                    ExpectUserResponse = false,
                    RichResponse = new GoogleRichResponse()
                    {
                        Items = new List<GoogleItem>()
                    }
                },
                Facebook = new FacebookPayloadSettings()
                {
                    Attachment = new FacebookAttachment()
                    {
                        Type = "template",
                        Payload = new FacebookPayload()
                        {
                            TemplateType = "generic",
                            ImageAspectRatio = "horizontal",
                            Elements = new List<FacebookElement>()
                        }
                    }
                }
            };

            payloadBuilder.Google.RichResponse.Items.Add(errorGoogleItem);

            try
            {
                var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
                var gsr = await _googleCustomSearch.GetResponse(searchTerm, marvelGoogleCx);

                var gsrMetaTags = gsr?.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0);

                if (gsrMetaTags != null)
                {
                    _logger.LogDebug("DialogFlow: GetMarvelPayload - Beginning to build card for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                    var facebookElement = new FacebookElement
                    {
                        Buttons = new List<FacebookButton>()
                    };

                    var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim() ?? searchTerm;
                    facebookElement.Title = title;

                    var bio = (!gsrMetaTags.OgDescription.IsNullOrWhiteSpace() ? gsrMetaTags.OgDescription : gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString()) ?? "Description unavailable at this time.";
                    facebookElement.Subtitle = bio;

                    var imageUrl = gsr.Items.ElementAtOrDefault(0)?.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                    facebookElement.ImageUrl = imageUrl;

                    var webSite = (!gsrMetaTags.OgUrl.IsNullOrWhiteSpace() ? gsrMetaTags.OgUrl : gsr.Items.ElementAtOrDefault(0)?.Link.ToNullIfEmpty()) ?? string.Empty;

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

                    payloadBuilder.Google.RichResponse.Items.Clear();
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

                    payloadBuilder.Google.RichResponse.Items.Add(successGoogleItem);
                    payloadBuilder.Google.RichResponse.Items.Add(successGoogleCard);

                    payloadBuilder.Facebook.Attachment.Payload.Elements.Add(facebookElement);

                    _logger.LogDebug("DialogFlow: GetMarvelPayload - Card built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetMarvelPayload - Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetMarvelPayload - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("DialogFlow: GetMarvelPayload - PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return payloadBuilder;
        }

        private async Task<PayloadBuilder> GetDCComicsPayload(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetDCComicsPayload - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/DCCard.png";

            var errorGoogleItem = new GoogleItem()
            {
                SimpleResponse = new GoogleSimpleResponse()
                {
                    TextToSpeech = $"Couldn't find anything for {searchTerm}"
                }
            };

            var payloadBuilder = new PayloadBuilder
            {
                Google = new GooglePayloadSettings
                {
                    ExpectUserResponse = false,
                    RichResponse = new GoogleRichResponse()
                    {
                        Items = new List<GoogleItem>()
                    }
                },
                Facebook = new FacebookPayloadSettings()
                {
                    Attachment = new FacebookAttachment()
                    {
                        Type = "template",
                        Payload = new FacebookPayload()
                        {
                            TemplateType = "generic",
                            ImageAspectRatio = "horizontal",
                            Elements = new List<FacebookElement>()
                        }
                    }
                }
            };

            payloadBuilder.Google.RichResponse.Items.Add(errorGoogleItem);

            try
            {
                var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
                var gsr = await _googleCustomSearch.GetResponse(searchTerm, dcComicsCx);

                var gsrMetaTags = gsr?.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0);

                if (gsrMetaTags != null)
                {
                    _logger.LogDebug("DialogFlow: GetDCComicsPayload - Beginning to build card for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                    var facebookElement = new FacebookElement
                    {
                        Buttons = new List<FacebookButton>()
                    };

                    var title = gsrMetaTags.OgTitle ?? searchTerm;
                    facebookElement.Title = title;

                    var bio = (!gsrMetaTags.OgDescription.IsNullOrWhiteSpace() ? gsrMetaTags.OgDescription : gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString()) ?? "Description unavailable at this time.";
                    facebookElement.Subtitle = bio;

                    var imageUrl = gsr.Items.ElementAtOrDefault(0)?.PageMap.CseThumbnail.ElementAtOrDefault(0)?.Src ?? backupImage;
                    facebookElement.ImageUrl = imageUrl;

                    var webSite = (!gsrMetaTags.OgUrl.IsNullOrWhiteSpace() ? gsrMetaTags.OgUrl : gsr.Items.ElementAtOrDefault(0)?.Link.ToNullIfEmpty()) ?? string.Empty;

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

                    payloadBuilder.Google.RichResponse.Items.Clear();
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

                    payloadBuilder.Google.RichResponse.Items.Add(successGoogleItem);
                    payloadBuilder.Google.RichResponse.Items.Add(successGoogleCard);

                    payloadBuilder.Facebook.Attachment.Payload.Elements.Add(facebookElement);

                    _logger.LogDebug("DialogFlow: GetDCComicsPayload - Card built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetDCComicsPayload - Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetDCComicsPayload - Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("DialogFlow: GetDCComicsPayload - PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return payloadBuilder;
        }

        private async Task<RepeatedField<Intent.Types.Message>> GetMarvelCard(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DialogFlow: GetMarvelCard - Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/Messenger/MarvelCard.png";

            var simpleResponse = new Intent.Types.Message.Types.SimpleResponse
            {
                TextToSpeech = $"Couldn't find anything for {searchTerm}"
            };

            var card = new Intent.Types.Message.Types.Card
            {
                Title = "Sorry",
                Subtitle = $"Couldn't find anything for {searchTerm}",
                ImageUri = backupImage
            };

            try
            {
                var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
                var gsr = await _googleCustomSearch.GetResponse(searchTerm, marvelGoogleCx);

                var gsrMetaTags = gsr?.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0);

                if (gsrMetaTags != null)
                {
                    _logger.LogDebug("DialogFlow: GetMarvelCard - Beginning to build card for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                    var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim() ?? searchTerm;
                    card.Title = title;

                    var bio = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
                    card.Subtitle = bio;

                    card.ImageUri = gsr.Items.ElementAtOrDefault(0)?.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;

                    card.Buttons.Add(new Intent.Types.Message.Types.Card.Types.Button()
                    {
                        Postback = gsrMetaTags.OgUrl,
                        Text = "Excelsior! Read more..."
                    });

                    simpleResponse.TextToSpeech = $"Excelsior! I found {title}! {bio}";

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

            var simpleResponses = new Intent.Types.Message.Types.SimpleResponses();
            simpleResponses.SimpleResponses_.Add(simpleResponse);

            var messages = new RepeatedField<Intent.Types.Message>
            {
                new Intent.Types.Message()
                {
                    Card = card,
                    SimpleResponses = simpleResponses
                },
                new Intent.Types.Message()
                {
                    SimpleResponses = simpleResponses
                }
            };

            return messages;
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