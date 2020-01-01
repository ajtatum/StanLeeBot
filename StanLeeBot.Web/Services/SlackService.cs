using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using SlackBotMessages;
using SlackBotMessages.Enums;
using SlackBotMessages.Models;
using SlackBotNet;
using SlackBotNet.State;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;
using static SlackBotNet.MatchFactory;

namespace StanLeeBot.Web.Services
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

        public async Task SendBotMessage()
        {
            var slackApiToken = _appSettings.Slack.ApiToken;

            var bot = await SlackBot.InitializeAsync(slackApiToken, cfg =>
            {
                cfg.LoggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory(Log.ForContext<SlackService>());
                cfg.OnSendMessageFailure = async (queue, msg, logger, e) =>
                {
                    if (msg.SendAttempts <= 5)
                    {
                        logger?.LogWarning("Failed to send message {MessageText}. Tried {SendAttempts} times", msg.Text, msg.SendAttempts);
                        await Task.Delay(1000 * msg.SendAttempts);
                        queue.Enqueue(msg);
                        return;
                    }

                    logger?.LogError("Gave up trying to send message {MessageText}", msg.Text);
                };
            });

            bot.When(Matches.Text("hello").Or(Matches.Text("hi").Or(Matches.Text("hola"))), HubType.DirectMessage | HubType.Channel | HubType.Group, async conv =>
            {
                _logger.LogInformation("StaLeeBot matched hello. {@Conversation}", conv);
                await conv.PostMessage($"Hi {conv.From.Username}!");
                conv.End();
            });

            bot.When(Matches.Text("test"), HubType.DirectMessage | HubType.Channel | HubType.Group, async conv =>
            {
                _logger.LogInformation("StaLeeBot matched test. {@Conversation}", conv);
                await conv.PostMessage($"I'm working {conv.From.Username}!");
                conv.End();
            });

            bot.When(Matches.Text("help"), HubType.DirectMessage | HubType.Channel | HubType.Group, async conv =>
            {
                _logger.LogInformation("StaLeeBot matched help. {@Conversation}", conv);
                await conv.PostMessage(BuildHelpText(conv.From.Username));
                conv.End();
            });

            bot.When(Matches.Text("support"), HubType.All, async conv =>
            {
                _logger.LogInformation("StanLeeBot matched support. {@Conversation}", conv);
                await conv.PostMessage("Sure, if you want to reach out go to https://stanleebot.com/Support");
                conv.End();
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

        public async Task GetStanLee(SlackCommandRequest slackCommandRequest)
        {
            var client = new SbmClient(slackCommandRequest.ResponseUrl);
            var message = new Message
            {
                Text = slackCommandRequest.Text switch
                {
                    "help" => BuildHelpText(slackCommandRequest.UserName),
                    "support" => "Sure, if you want to reach out go to https://stanleebot.com/Support",
                    _ => "Unfortunately, I only know how to respond to help and support right now."
                }
            };
            
            var response = await client.SendAsync(message);

            if (response == "ok")
                _logger.LogInformation("GetStanLee: Responded to {MessageText} and sent message {@Message}", slackCommandRequest.Text, message);
            else
                _logger.LogError("GetStanLee: Tried responding to {MessageText} but received an error sending {@Message}", slackCommandRequest.Text, message);
        }
        #endregion

        private string BuildHelpText(string username)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Never fear {username} I can help!");
            stringBuilder.AppendLine("To learn more about anything Marvel or DC Comics related, use the slash commands /marvel and /dc.");
            stringBuilder.AppendLine("For example, if you want to lookup Ironman you'd type /marvel Ironman");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("If you're looking for support, go to https://stanleebot.com/Support");

            return stringBuilder.ToString();
        }

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