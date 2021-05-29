using System.Threading.Tasks;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Services.Interfaces
{
    public interface IGoogleSearchService
    {
        Task<GoogleSearchResponse.SearchResponse> GetResponse(string search, string cse);
    }
}
