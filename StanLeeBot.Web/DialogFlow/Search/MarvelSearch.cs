using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StanLeeBot.Web.Areas.API;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web.DialogFlow.Search
{
    public class MarvelSearch
    {
        private readonly ILogger<MarvelSearch> _logger;
        private readonly AppSettings _appSettings;
        private readonly IGoogleCustomSearch _googleCustomSearch;

        public MarvelSearch(ILogger<MarvelSearch> logger, IOptionsMonitor<AppSettings> appSettings, IGoogleCustomSearch googleCustomSearch)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _googleCustomSearch = googleCustomSearch;
        }
    }
}
