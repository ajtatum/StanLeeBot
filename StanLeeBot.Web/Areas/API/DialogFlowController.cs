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
using StanLeeBot.Web.Models;
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

                    var intentName = webhookRequest.QueryResult.Intent.Name;
                    intentName = intentName.Substring(intentName.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    var queryText = webhookRequest.QueryResult.QueryText;

                    _logger.LogInformation("DialogFlow: Processing request for intent {Intent} and queryText {QueryText}", intentName, queryText);

                    var webHookResponse = new WebhookResponse();

                    switch (intentName)
                    {
                        case Constants.DialogFlow.HelloMarvelLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request Marvel Lookup.");
                            webHookResponse.FulfillmentMessages.Add(await GetMarvelCard(queryText));
                            break;
                        case Constants.DialogFlow.HelloDCLookupId:
                            _logger.LogDebug("DialogFlow: Beginning to request DC Comics Lookup.");
                            webHookResponse.FulfillmentMessages.Add(await GetDcComicsCard(queryText));
                            break;
                        default:
                            _logger.LogDebug("DialogFlow: Unhandled intent: {IntentName}", intentName);
                            webHookResponse.FulfillmentText = "Sorry, but I don't understand.";
                            break;
                    }

                    return new OkObjectResult(webHookResponse);
                }

                if (authHeader.IsNullOrWhiteSpace())
                    _logger.LogError("DialogFlow: Request header is absent.");
                else
                    _logger.LogError("DialogFlow: Request header do not match. Value sent: {AuthHeader}", authHeader);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: Error during request.");
                return new BadRequestResult();
            }

            return new UnauthorizedResult();
        }

        private async Task<RepeatedField<Intent.Types.Message>> GetMarvelCard(string searchTerm)
        {
            _logger.LogInformation("DialogFlow: GetMarvelCard - Received request to search for {SearchTerm}", searchTerm);

            var backupImage = "https://stanleebot.com/images/DialogFlow/Messenger/MarvelCard.png";

            var marvelCard = new Intent.Types.Message.Types.Card
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
                    _logger.LogDebug("DialogFlow: GetMarvelCard - Beginning to build card for {SearchTerm}.", searchTerm);

                    var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim() ?? searchTerm;
                    marvelCard.Title = title;

                    var bio = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
                    marvelCard.Subtitle = bio;

                    marvelCard.ImageUri = gsr.Items.ElementAtOrDefault(0)?.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;

                    marvelCard.Buttons.Add(new Intent.Types.Message.Types.Card.Types.Button()
                    {
                        Postback = gsrMetaTags.OgUrl,
                        Text = "Excelsior! Read more..."
                    });

                    _logger.LogDebug("DialogFlow: GetMarvelCard - Card built for {SearchTerm}.", searchTerm);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetMarvelCard - Metatags are null for {SearchTerm}", searchTerm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetMarvelCard - Error during search request for {SearchTerm}.", searchTerm);
            }

            var messages = new RepeatedField<Intent.Types.Message>
            {
                new Intent.Types.Message()
                {
                    Card = marvelCard
                }
            };

            return messages;
        }

        private async Task<RepeatedField<Intent.Types.Message>> GetDcComicsCard(string searchTerm)
        {
            _logger.LogInformation("DialogFlow: GetDcComicsCard - Received request to search for {SearchTerm}", searchTerm);

            var backupImage = "https://stanleebot.com/images/DialogFlow/Messenger/DCCard.png";

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
                    _logger.LogDebug("DialogFlow: GetDcComicsCard - Beginning to build card for {SearchTerm}.", searchTerm);

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

                    _logger.LogDebug("DialogFlow: GetDcComicsCard - Card built for {SearchTerm}.", searchTerm);
                }
                else
                {
                    _logger.LogError("DialogFlow: GetDcComicsCard - Metatags are null for {SearchTerm}", searchTerm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogFlow: GetDcComicsCard - Error during search request for {SearchTerm}.", searchTerm);
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