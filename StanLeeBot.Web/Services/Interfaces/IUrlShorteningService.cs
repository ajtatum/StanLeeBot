using System.Threading.Tasks;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Services.Interfaces
{
    public interface IUrlShorteningService
    {
        Task<string> Shorten(string longUrl, string domain, string emailAddress, OriginSources originSource, string sessionId);
    }
}
