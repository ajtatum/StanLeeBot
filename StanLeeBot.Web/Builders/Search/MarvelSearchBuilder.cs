using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StanLeeBot.Web.Builders.Search.Interfaces;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;
using StanLeeBot.Web.Services.Interfaces;


namespace StanLeeBot.Web.Builders.Search
{
    public class MarvelSearchBuilder : ISearchBuilder<MarvelSearchBuilder>
    {
        private readonly ILogger<MarvelSearchBuilder> _logger;
        private readonly AppSettings _appSettings;
        private readonly IGoogleSearchService _googleSearchService;

        public MarvelSearchBuilder(ILogger<MarvelSearchBuilder> logger, IOptionsMonitor<AppSettings> appSettings, IGoogleSearchService googleSearchService)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _googleSearchService = googleSearchService;
        }

        public async Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string searchTerm, string sessionId)
        {
            _logger.LogInformation("MarvelBuilder: Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/MarvelCard.png";

            #region Set Defaults
            var fulfillmentMessage = new DialogFlowResponse.FulfillmentMessage
            {
                Card = new DialogFlowResponse.Card()
                {
                    Title = "Sorry",
                    Subtitle = $"Couldn't find anything for {searchTerm}",
                    ImageUri = backupImage,
                    Buttons = new List<DialogFlowResponse.Button>()
                }
            };

            var fulfillmentText = $"Couldn't find anything for {searchTerm}";

            var payloadBuilder = Payloads.DefaultPayload.Build(searchTerm);
            #endregion

            try
            {
                var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
                var marvelGsr = await _googleSearchService.GetResponse(searchTerm, marvelGoogleCx);

                if (marvelGsr.Items != null)
                {
                    var firstGsrItem = marvelGsr.Items.ElementAt(0);
                    var gsrMetaTags = firstGsrItem.PageMap.MetaTags.ElementAtOrDefault(0);

                    if (gsrMetaTags != null)
                    {
                        _logger.LogInformation("MarvelBuilder: Beginning to build payload for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        var title = CleanTitle(gsrMetaTags.OgTitle) ?? CleanTitle(firstGsrItem.Title) ?? searchTerm;
                        var bio = gsrMetaTags.OgDescription.ToNullIfWhiteSpace() ?? firstGsrItem.Snippet.CleanString().ToNullIfWhiteSpace() ?? "Description unavailable at this time.";
                        var imageUrl = firstGsrItem.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                        var webSite = gsrMetaTags.OgUrl.ToNullIfWhiteSpace() ?? firstGsrItem.Link.ToNullIfWhiteSpace() ?? "https://www.marvel.com/";

                        var responseCard = new DialogFlowResponse.Card
                        {
                            Title = title,
                            Subtitle = bio,
                            ImageUri = imageUrl,
                            Buttons = new List<DialogFlowResponse.Button>()
                            {
                                new DialogFlowResponse.Button()
                                {
                                    PostBack = webSite,
                                    Text = "Excelsior! Read more..."
                                }
                            }
                        };

                        fulfillmentText = $"Excelsior! I found {title}! {bio}";
                        fulfillmentMessage.Card = responseCard;

                        payloadBuilder.Google = Payloads.GooglePayload.Build(title, bio, imageUrl, webSite);
                        payloadBuilder.Facebook = Payloads.FacebookPayload.Build(title, bio, imageUrl, webSite, FacebookPayloadSettings.ImageAspectRatioEnum.Horizontal, FacebookPayloadSettings.WebViewHeightRatioEnum.Full);

                        _logger.LogInformation("MarvelBuilder: Payload built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        _logger.LogDebug("MarvelBuilder: PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);
                        return (fulfillmentText, fulfillmentMessage, payloadBuilder);
                    }

                    _logger.LogError("MarvelBuilder: Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("MarvelBuilder: There are no items for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarvelBuilder: Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("MarvelBuilder: Default PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return (fulfillmentText, fulfillmentMessage, payloadBuilder);

            static string CleanTitle(string title)
            {
                return title.IsNullOrWhiteSpace()
                    ? null
                    : title.Split('|').ElementAtOrDefault(0)?.Trim().ToNullIfWhiteSpace();
            }
        }
    }
}
