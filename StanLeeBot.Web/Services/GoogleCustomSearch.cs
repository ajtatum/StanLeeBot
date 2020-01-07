using System;
using System.Net.Http;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web.Services
{
    public class GoogleCustomSearch : IGoogleCustomSearch
    {
        private readonly ILogger<GoogleCustomSearch> _logger;
        private readonly AppSettings _appSettings;

        public GoogleCustomSearch(ILogger<GoogleCustomSearch> logger, IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
        }

        public async Task<GoogleSearchResponse> GetResponse(string search, string cse)
        {
            var googleApiKey = _appSettings.GoogleCustomSearch.ApiKey;

            var url = $"https://www.googleapis.com/customsearch/v1?cx={cse}&key={googleApiKey}&q={search}";
            var result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    result = await client.GetStringAsync(url);
                }

                if (!result.IsNullOrWhiteSpace())
                {
                    var googleSearchResponse = JsonConvert.DeserializeObject<GoogleSearchResponse>(result);
                    return googleSearchResponse;
                }

                _logger.LogError("Tried searching for {SearchTerm} with {CSE} but the results were empty", search, cse);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Using search {Url} return result: {Result}", url, result);
                return null;
            }
        }
    }
}
