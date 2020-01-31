using System.Threading.Tasks;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;

namespace StanLeeBot.Web.Builders.ShortenUrl.Interfaces
{
    public interface IShortenUrlBuilder
    {
        Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string longUrl, string domain, string sessionId);
    }
}
