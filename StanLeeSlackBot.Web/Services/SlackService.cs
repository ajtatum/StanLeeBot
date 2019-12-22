using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlackBotMessages;
using SlackBotMessages.Enums;
using SlackBotMessages.Models;
using SlackBotNet;
using SlackBotNet.State;
using StanLeeSlackBot.Web.Models;
using StanLeeSlackBot.Web.Services.Interfaces;
using static SlackBotNet.MatchFactory;

namespace StanLeeSlackBot.Web.Services
{
    public class SlackService : ISlackService
    {
        private readonly ILogger<SlackService> _logger;
        private readonly AppSettings _appSettings;
        
        public SlackService(ILogger<SlackService> logger, IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
        }

        public void SendBotMessage()
        {
            var slackApiToken = _appSettings.Slack.ApiToken;

            var bot = SlackBot.InitializeAsync(slackApiToken).Result;

            bot.When(Matches.Text("hello"), HubType.DirectMessage | HubType.Channel | HubType.Group, async conv =>
            {
                _logger.LogInformation("StaLeeBot matched hello. {@Conversation}", conv);
                await conv.PostMessage($"Hi {conv.From.Username}!");
            });

            bot.When(Matches.Text("test"), HubType.DirectMessage | HubType.Channel | HubType.Group, async conv =>
            {
                _logger.LogInformation("StaLeeBot matched test. {@Conversation}", conv);
                await conv.PostMessage($"I'm working {conv.From.Username}!");
            });
        }

        #region SlackSlashCommands
        public async Task GetMarvel(SlackCommandRequest slackCommandRequest)
        {
            var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
            var gsr = await GetGoogleSearchSlackResponseJson(slackCommandRequest.Text, marvelGoogleCx);

            if (gsr != null)
            {
                var client = new SbmClient(slackCommandRequest.ResponseUrl);
                var message = new Message();

                var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new MetaTag();
                var snippet = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;

                var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim();

                var attachment = new Attachment()
                {
                    Fallback = snippet,
                    Pretext = $"Excelsior! I found {title} :star-struck:!"
                }
                    .AddField("Name", title, true)
                    .AddField("Website", gsrMetaTags.OgUrl, true)
                    .AddField("Bio", snippet)
                    .SetImage(gsr.Items[0].PageMap.CseImage[0].Src)
                    .SetColor(Color.Green);

                message.AddAttachment(attachment);
                var response = await client.SendAsync(message);

                if (response == "ok")
                    _logger.LogInformation("GetMarvel: Sent message {@Message}", message);
                else
                    _logger.LogError("GetMarvel: Error sending {@Message}", message);
            }
            else
            {
                _logger.LogError("GetMarvel: GetGoogleSearchSlackResponseJson is null. {@SlackCommandRequest}", slackCommandRequest);
            }
        }

        public async Task GetDcComics(SlackCommandRequest slackCommandRequest)
        {
            var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
            var gsr = await GetGoogleSearchSlackResponseJson(slackCommandRequest.Text, dcComicsCx);

            if (gsr != null)
            {
                var client = new SbmClient(slackCommandRequest.ResponseUrl);
                var message = new Message();

                var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new MetaTag();
                var snippet = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
                var characterName = gsrMetaTags.OgTitle;

                var attachment = new Attachment()
                {
                    Fallback = snippet,
                    Pretext = $"Excelsior! I found {characterName} :star-struck:!"
                }
                    .AddField("Name", characterName, true)
                    .AddField("Website", gsrMetaTags.OgUrl, true)
                    .AddField("Bio", snippet)
                    .SetThumbUrl(gsr.Items[0].PageMap.CseThumbnail.ElementAtOrDefault(0)?.Src)
                    .SetColor(Color.Green);

                message.AddAttachment(attachment);
                var response = await client.SendAsync(message);

                if (response == "ok")
                    _logger.LogInformation("GetDCComics: Sent message {@Message}", message);
                else
                    _logger.LogError("GetDCComics: Error sending {@Message}", message);
            }
            else
            {
                _logger.LogError("GetDCComics: GetGoogleSearchSlackResponseJson is null. {@SlackCommandRequest}", slackCommandRequest);
            }
        }
        #endregion

        private async Task<GoogleSearchResponse> GetGoogleSearchSlackResponseJson(string search, string cse)
        {
            var googleApiKey = _appSettings.GoogleCustomSearch.ApiKey;

            var url = $"https://www.googleapis.com/customsearch/v1?cx={cse}&key={googleApiKey}&q={search}";
            var result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    result = await client.GetStringAsync(url);
                }

                var googleSearchResponse = JsonConvert.DeserializeObject<GoogleSearchResponse>(result);
                return googleSearchResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Using search {Url} return result: {Result}", url, result);
                return null;
            }
        }

    }
}