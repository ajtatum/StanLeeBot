using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
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

        //public async Task SendBotMessage()
        //{
        //    var slackApiToken = _appSettings.Slack.ApiToken;

        //    var bot = await SlackBot.InitializeAsync(slackApiToken, cfg =>
        //    {
        //        cfg.LoggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory(Log.ForContext<SlackService>());
        //        cfg.OnSendMessageFailure = async (queue, msg, logger, e) =>
        //        {
        //            if (msg.SendAttempts <= 5)
        //            {
        //                logger?.LogWarning("Failed to send message {MessageText}. Tried {SendAttempts} times", msg.Text, msg.SendAttempts);
        //                await Task.Delay(1000 * msg.SendAttempts);
        //                queue.Enqueue(msg);
        //                return;
        //            }

        //            logger?.LogError("Gave up trying to send message {MessageText}", msg.Text);
        //        };
        //    });

        //    bot.When(Matches.Text("help"), HubType.DirectMessage, async conv =>
        //    {
        //        _logger.LogInformation("StanLeeBot matched help in a DM");
        //        await conv.PostMessage(BuildHelpText(conv.From.Username)).ConfigureAwait(false);
        //    });

        //    bot.When(Matches.Text("support"), HubType.DirectMessage, async conv =>
        //    {
        //        _logger.LogInformation("StanLeeBot matched support in a DM");
        //        await conv.PostMessage("Sure, if you want to reach out go to https://stanleebot.com/Support").ConfigureAwait(false);
        //    });
        //}

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
                
                var snippet = gsrMetaTags.OgDescription;
                if (snippet.IsNullOrWhiteSpace())
                {
                    snippet = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
                }

                var title = gsr.Items.ElementAtOrDefault(0)?.Title.Split("|").ElementAtOrDefault(0)?.Trim();

                var website = gsrMetaTags.OgUrl;
                if (website.IsNullOrWhiteSpace())
                {
                    website = gsr.Items.ElementAtOrDefault(0)?.Link;
                }

                var attachment = new Attachment()
                {
                    Fallback = snippet,
                    Pretext = $"Excelsior! I found {title} :star-struck:!"
                }
                    .AddField("Name", title, true)
                    .AddField("Website", website, true)
                    .AddField("Bio", snippet)
                    .SetImage(gsr.Items[0].PageMap.CseImage[0].Src)
                    .SetColor(Color.Green);

                message.SetResponseType(ResponseType.InChannel);
                message.AddAttachment(attachment);
                var response = await client.SendAsync(message).ConfigureAwait(false);

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

                message.SetResponseType(ResponseType.InChannel);
                message.AddAttachment(attachment);
                var response = await client.SendAsync(message).ConfigureAwait(false);

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
                UnfurlLinks = false,
                UnfurlMedia = false,
                Text = slackCommandRequest.Text switch
                {
                    "help" => BuildHelpText(slackCommandRequest.UserName),
                    "support" => "Sure, if you want to reach out go to https://stanleebot.com/Support",
                    _ => "Unfortunately, I only know how to respond to help and support right now."
                }
            };

            message.SetResponseType(ResponseType.Ephemeral);
            var response = await client.SendAsync(message).ConfigureAwait(false);

            if (response == "ok")
                _logger.LogInformation("GetStanLee: Responded to {MessageText} and sent message {@Message}", slackCommandRequest.Text, message);
            else
                _logger.LogError("GetStanLee: Tried responding to {MessageText} but received an error sending {@Message}", slackCommandRequest.Text, message);
        }

        public async Task GetMrvlCoLink(SlackCommandRequest slackCommandRequest)
        {
            var client = new SbmClient(slackCommandRequest.ResponseUrl);
            var firstResponse = new Message();
            firstResponse.SetResponseType(ResponseType.Ephemeral);
            firstResponse.Text = "Sure thing! Let me get to work!";
            await client.SendAsync(firstResponse).ConfigureAwait(false);


            var message = new Message();
            message.SetResponseType(ResponseType.Ephemeral);
            message.UnfurlLinks = false;
            message.UnfurlMedia = false;

            var response = string.Empty;
            var textList = slackCommandRequest.Text.Split(" ").ToList();
            if (textList.Count() == 2)
            {
                var longUrl = textList[0];
                var emailAddress = textList[1];

                var urlIsValid = longUrl.IsValidUrl();
                var emailIsValid = new EmailAddressAttribute().IsValid(emailAddress);

                if (urlIsValid && emailIsValid)
                {
                    var restClient = new RestClient(_appSettings.UrlShortenerEndpoint);
                    var restRequest = new RestRequest(Method.POST);
                    restRequest.AddHeader("AuthKey", _appSettings.BabouAuthKeys.Slack);
                    restRequest.AddHeader("longUrl", longUrl);
                    restRequest.AddHeader("emailAddress", emailAddress);
                    var restResponse = await restClient.ExecuteTaskAsync(restRequest);

                    if (!restResponse.ErrorMessage.IsNullOrWhiteSpace())
                    {
                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine($"There was an error while processing your request. The error message is {restResponse.ErrorMessage}.");
                        stringBuilder.AppendLine("If you continue to receive this error, please contact us at https://stanleebot.com/Support.");

                        message.Text = stringBuilder.ToString();
                        response = await client.SendAsync(message).ConfigureAwait(false);
                    }
                    else
                    {
                        var shortUrl = restResponse.Content;

                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine($"I've shortened {longUrl} to {shortUrl}");
                        stringBuilder.AppendLine("If you want to manage your short url or view stats, go to https://babou.io.");
                        stringBuilder.AppendLine($"If you haven't already, use the \"Forgot Password\" link on the login page and enter your email address ({emailAddress}) to reset your password.");

                        message.Text = stringBuilder.ToString();
                        response = await client.SendAsync(message).ConfigureAwait(false);
                    }
                }
                else if (!urlIsValid)
                {
                    message.Text = $"The url you provided, {longUrl}, doesn't seem to be a valid URL. Please try again.";
                    response = await client.SendAsync(message).ConfigureAwait(false);
                }
                else if (!emailIsValid)
                {
                    message.Text = $"The email address you provided, {emailAddress}, doesn't seem to be a valid email. Please try again.";
                    response = await client.SendAsync(message).ConfigureAwait(false);
                }
            }
            else
            {
                message.Text = "Please verify that your message is in the format of URL<space>EmailAddress";
                response = await client.SendAsync(message).ConfigureAwait(false);
            }

            if (response == "ok")
                _logger.LogInformation("GetMrvlCoLink: Responded to {MessageText} and sent message {@Message}", slackCommandRequest.Text, message);
            else
                _logger.LogError("GetMrvlCoLink: Tried responding to {MessageText} but received an error sending {@Message}", slackCommandRequest.Text, message);

        }
        #endregion

        private string BuildHelpText(string username)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Never fear {username} I can help!");
            stringBuilder.AppendLine("To learn more about anything Marvel or DC Comics related, use the slash commands */marvel* and */dc*.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Here are some examples:");
            stringBuilder.AppendLine("* /marvel Ironman");
            stringBuilder.AppendLine("* /marvel Infinity Stones");
            stringBuilder.AppendLine("* /dc Batman");
            stringBuilder.AppendLine("* /dc Justice League Movie");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("I can also shorten long urls using the command */mrvlco*. Here's the format:");
            stringBuilder.AppendLine("* /mrvlco LongUrl YourEmailAddress");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("This will generate a https://mrvl.co/ url.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("I ask for your email so that if you want to keep track of your short urls or view stats, ");
            stringBuilder.Append("you can log into the URL Shortening Service provided by https://babou.io.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("If you have any other questions, concerns, or need more help go to https://stanleebot.com/Support");

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