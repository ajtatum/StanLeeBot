using System.Threading.Tasks;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Services.Interfaces
{
    public interface IGoogleCustomSearch
    {
        Task<GoogleSearchResponse> GetResponse(string search, string cse);
    }
}
