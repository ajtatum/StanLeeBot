using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StanLeeBot.Web.Services
{
    public class TelegramMessagingService : ITelegramMessagingService
    {
        private readonly ITelegramBotService _botService;
        private readonly ILogger<TelegramMessagingService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IGoogleSearchService _googleSearchService;

        public TelegramMessagingService(ITelegramBotService botService, ILogger<TelegramMessagingService> logger,
                                        IOptionsMonitor<AppSettings> appSettings, IGoogleSearchService googleSearchService)
        {
            _botService = botService;
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _googleSearchService = googleSearchService;
        }

        public async Task HandleMessage(Update update)
        {
            if (update.Type != UpdateType.Message)
                return;

            var message = update.Message;

            _logger.LogInformation("Telegram received message type {MessageType}", message.Type.GetDisplayName());

            switch (message.Type)
            {
                case MessageType.Text:
                    _logger.LogInformation("Received Telegram Message from {Username} with {Text}", message.Chat.Username, message.Text);

                    if (message.Text == "/marvel" || message.Text == "/dc")
                    {
                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Please specify what you're looking for. For example, \"/marvel Ironman\"");
                    }
                    else if (message.Text.StartsWith("/marvel"))
                    {
                        var lookingFor = message.Text.Replace("/marvel", string.Empty).Trim();
                        var searchInfo = await GetMarvel(lookingFor);

                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, searchInfo);
                    }
                    else if (message.Text.StartsWith("/dc"))
                    {
                        var lookingFor = message.Text.Replace("/dc", string.Empty).Trim();
                        var searchInfo = await GetDcComics(lookingFor);

                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, searchInfo);
                    }
                    else
                    {
                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Excelsior! I'm at your service!");
                    }
                    break;
                case MessageType.Video:
                case MessageType.Photo:
                    if (!message.Caption.IsNullOrWhiteSpace())
                    {
                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, $"Great pic/video of {message.Caption}!");
                    }
                    else
                    {
                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Great pic/video!");
                    }
                    break;
            }
        }

        public async Task<string> GetMarvel(string lookingFor)
        {
            var marvelGoogleCx = _appSettings.GoogleCustomSearch.MarvelCx;
            var gsr = await _googleSearchService.GetResponse(lookingFor, marvelGoogleCx);

            var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new GoogleSearchResponse.MetaTag();

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

        public async Task<string> GetDcComics(string lookingFor)
        {
            var dcComicsCx = _appSettings.GoogleCustomSearch.DcComicsCx;
            var gsr = await _googleSearchService.GetResponse(lookingFor, dcComicsCx);

            var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new GoogleSearchResponse.MetaTag();

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
