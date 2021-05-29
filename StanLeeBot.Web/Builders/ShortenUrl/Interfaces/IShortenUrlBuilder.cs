using System.Threading.Tasks;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Models.DialogFlow;
using StanLeeBot.Web.Models.DialogFlow.Payloads;
using StanLeeBot.Web.Services;

namespace StanLeeBot.Web.Builders.ShortenUrl.Interfaces
{
    public interface IShortenUrlBuilder
    {
        Task<(string, DialogFlowResponse.FulfillmentMessage, PayloadSettings)> Build(string longUrl, string domain, string emailAddress, OriginSources originSource, string sessionId);
    }
}
