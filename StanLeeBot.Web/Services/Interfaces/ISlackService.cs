using System.Threading.Tasks;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Services.Interfaces
{
    public interface ISlackService
    {
        Task SendBotMessage();
        Task GetMarvel(SlackCommandRequest slackCommandRequest);
        Task GetDcComics(SlackCommandRequest slackCommandRequest);
        Task GetStanLee(SlackCommandRequest slackCommandRequest);
    }
}
