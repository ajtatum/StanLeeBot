using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace StanLeeBot.Web.Services.Interfaces
{
    public interface ITelegramMessagingService
    {
        Task HandleMessage(Update update);
    }
}
