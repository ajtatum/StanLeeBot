using System.Threading.Tasks;
using BabouExtensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;
using Telegram.Bot.Types;

namespace StanLeeBot.Web.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramMessagingService _messagingService;
        private readonly AppSettings _appSettings;

        public TelegramController(ITelegramMessagingService messagingService, IOptionsMonitor<AppSettings> appSettings)
        {
            _messagingService = messagingService;
            _appSettings = appSettings.CurrentValue;
        }

        [HttpGet]
        public IActionResult Get(string apiToken)
        {
            if(apiToken != _appSettings.Telegram.ApiToken)
                return new UnauthorizedObjectResult("Bad API Token");

            return new OkObjectResult("Welcome to the Telegram API");
        }

        [HttpPost]
        public async Task<IActionResult> Post(string apiToken)
        {
            if (apiToken != _appSettings.Telegram.ApiToken)
                return new UnauthorizedObjectResult("Bad API Token");

            var request = await Request.GetRawBodyStringAsync();

            var update = JsonConvert.DeserializeObject<Update>(request);

            await _messagingService.HandleMessage(update);
            return Ok();
        }
    }
}