using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StanLeeSlackBot.Web.Models;
using StanLeeSlackBot.Web.Services.Interfaces;

namespace StanLeeSlackBot.Web.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        private readonly ILogger<SlackController> _logger;
        private readonly AppSettings _appSettings;
        private readonly ISlackService _slackService;

        public SlackController(ILogger<SlackController> logger, IOptionsMonitor<AppSettings> appSettings, ISlackService slackService)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
            _slackService = slackService;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            Request.EnableBuffering();
            var requestBody = await Request.GetRawBodyStringAsyncWithOptions(null, null, true);
            var slackSignature = Request.Headers["X-Slack-Signature"].ToString();
            var slackRequestTimeStamp = Request.Headers["X-Slack-Request-Timestamp"].ToString();

            var fromSlack = IsRequestFromSlack(slackSignature, slackRequestTimeStamp, requestBody);

            if (!fromSlack)
                return new BadRequestObjectResult("Request is not from Slack");

            var requestForm = await Request.ReadFormAsync();
            var slackCommandRequest = new SlackCommandRequest()
            {
                Token = requestForm["token"],
                TeamId = requestForm["team_id"],
                TeamDomain = requestForm["team_domain"],
                ChannelId = requestForm["channel_id"],
                ChannelName = requestForm["channel_name"],
                UserId = requestForm["user_id"],
                UserName = requestForm["user_name"],
                Command = requestForm["command"],
                Text = requestForm["text"],
                ResponseUrl = requestForm["response_url"],
                TriggerId = requestForm["trigger_id"]
            };

            _logger.LogInformation("SlackCommandRequest: {@SlackCommandRequest}", slackCommandRequest);

            switch (slackCommandRequest.Command)
            {
                case "/marvel":
                    await _slackService.GetMarvel(slackCommandRequest);
                    return new OkResult();
                case "/dc":
                    await _slackService.GetDcComics(slackCommandRequest);
                    return new OkResult();
                default:
                    return new BadRequestResult();
            }
        }

        private bool IsRequestFromSlack(string slackSignature, string slackRequestTimeStamp, string requestBody)
        {
            var baseString = $"v0:{slackRequestTimeStamp}:{requestBody}";
            var signingSecret = _appSettings.Slack.SigningSecret;
            var computedHash = GetHash(baseString, signingSecret);

            return $"v0={computedHash}" == slackSignature;
        }

        private static string GetHash(string baseString, string key)
        {
            var encoding = new ASCIIEncoding();

            var textBytes = encoding.GetBytes(baseString);
            var keyBytes = encoding.GetBytes(key);

            byte[] hashBytes;

            using (var hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}