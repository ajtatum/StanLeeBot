using System.Threading.Tasks;
using StanLeeSlackBot.Web.Models;

namespace StanLeeSlackBot.Web.Services.Interfaces
{
    public interface ISlackService
    {
        void SendBotMessage();
        Task GetMarvel(SlackCommandRequest slackCommandRequest);
        Task GetDcComics(SlackCommandRequest slackCommandRequest);
    }
}
