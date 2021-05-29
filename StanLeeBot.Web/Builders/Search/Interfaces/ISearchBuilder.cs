using System.Threading.Tasks;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.Search.Interfaces
{
    public interface ISearchBuilder<out T>
    {
        Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string searchTerm, string sessionId);
    }
}
