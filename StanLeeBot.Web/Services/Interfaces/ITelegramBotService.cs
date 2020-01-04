using Telegram.Bot;

namespace StanLeeBot.Web.Services.Interfaces
{
    public interface ITelegramBotService
    {
        TelegramBotClient Client { get; }
    }
}
