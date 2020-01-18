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
    public class DCComicsSearchBuilder : ISearchBuilder<DCComicsSearchBuilder>
    {
        private readonly ILogger<DCComicsSearchBuilder> _logger;
        private readonly AppSettings _appSettings;
        private readonly IGoogleSearchService _googleSearchService;

        public DCComicsSearchBuilder(ILogger<DCComicsSearchBuilder> logger, IOptionsMonitor<AppSettings> appSettings, IGoogleSearchService googleSearchService)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _googleSearchService = googleSearchService;
        }

        public async Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string searchTerm, string sessionId)
        {
            _logger.LogInformation("DCComicsSearchBuilder: Received request to search for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

            const string backupImage = "https://stanleebot.com/images/DialogFlow/DCCard.png";

            #region Set Defaults
            var responseFulfillmentMessage = new DialogFlowResponse.FulfillmentMessage
            {
                Card = new DialogFlowResponse.Card()
                {
                    Title = "Sorry",
                    Subtitle = $"Couldn't find anything for {searchTerm}",
                    ImageUri = backupImage,
                    Buttons = new List<DialogFlowResponse.Button>()
                }
            };

            var responseFulfillmentText = $"Couldn't find anything for {searchTerm}";

            var payloadBuilder = Payloads.DefaultPayload.Build(searchTerm);
            #endregion

            try
            {
                var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
                var dcGsr = await _googleSearchService.GetResponse(searchTerm, dcComicsCx);

                if (dcGsr.Items != null)
                {
                    var firstGsrItem = dcGsr.Items.ElementAt(0);
                    var gsrMetaTags = firstGsrItem.PageMap.MetaTags.ElementAtOrDefault(0);

                    if (gsrMetaTags != null)
                    {
                        _logger.LogInformation("DCComicsSearchBuilder: Beginning to build payload for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        var title = gsrMetaTags.OgTitle.ToNullIfWhiteSpace() ?? firstGsrItem.Title.ToNullIfWhiteSpace() ?? searchTerm;
                        var bio = gsrMetaTags.OgDescription.ToNullIfWhiteSpace() ?? firstGsrItem.Snippet.CleanString().ToNullIfWhiteSpace() ?? "Description unavailable at this time.";
                        var imageUrl = firstGsrItem.PageMap.CseImage.ElementAtOrDefault(0)?.Src ?? backupImage;
                        var webSite = gsrMetaTags.OgUrl.ToNullIfWhiteSpace() ?? firstGsrItem.Link.ToNullIfWhiteSpace() ?? "https://www.dccomics.com/";

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

                        responseFulfillmentText = $"Excelsior! I found {title}! {bio}";
                        responseFulfillmentMessage.Card = responseCard;

                        payloadBuilder.Google = Payloads.GooglePayload.Build(title, bio, imageUrl, webSite);
                        payloadBuilder.Facebook = Payloads.FacebookPayload.Build(title, bio, imageUrl, webSite, FacebookPayloadSettings.ImageAspectRatioEnum.Square, FacebookPayloadSettings.WebViewHeightRatioEnum.Tall);

                        _logger.LogInformation("DCComicsSearchBuilder: Payload built for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);

                        _logger.LogDebug("DCComicsSearchBuilder: PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);
                        return (responseFulfillmentText, responseFulfillmentMessage, payloadBuilder);
                    }

                    _logger.LogError("DCComicsSearchBuilder: Metatags are null for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
                else
                {
                    _logger.LogError("DCComicsSearchBuilder: There are no items for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DCComicsSearchBuilder: Error during search request for {SearchTerm}. SessionId: {SessionId}.", searchTerm, sessionId);
            }

            _logger.LogDebug("DCComicsSearchBuilder: Default PayloadBuilder: {@PayloadBuilder}. SessionId: {SessionId}.", payloadBuilder, sessionId);

            return (responseFulfillmentText, responseFulfillmentMessage, payloadBuilder);
        }
    }
}
