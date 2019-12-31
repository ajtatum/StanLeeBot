using System.Threading.Tasks;
using StanLeeSlackBot.Web.Models;

namespace StanLeeSlackBot.Web.Services.Interfaces
{
    public interface ISlackService
    {
        Task SendBotMessage();
        Task GetMarvel(SlackCommandRequest slackCommandRequest);
        Task GetDcComics(SlackCommandRequest slackCommandRequest);
        Task GetStanLee(SlackCommandRequest slackCommandRequest);
    }
}
