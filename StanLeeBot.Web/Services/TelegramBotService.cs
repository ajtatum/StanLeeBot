using Microsoft.Extensions.Options;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;
using Telegram.Bot;

namespace StanLeeBot.Web.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly AppSettings _appSettings;
        public TelegramBotClient Client { get; }

        public TelegramBotService(IOptionsMonitor<AppSettings> appSettings)
        {
            _appSettings = appSettings.CurrentValue;
            Client = new TelegramBotClient(_appSettings.Telegram.ApiToken);
        }
    }
}
