using System;
using System.Net.Http;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlackBotMessages;
using SlackBotMessages.Models;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly AppSettings _appSettings;

        public AuthenticationController(ILogger<AuthenticationController> logger, IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
        }

        [HttpGet("~/login")]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Slack");
        }

        [HttpGet("~/signin-slack")]
        public async Task<IActionResult> SignInSlack()
        {
            try
            {
                var clientId = _appSettings.Slack.ClientId;
                var clientSecret = _appSettings.Slack.ClientSecret;
                var slackCode = Request.Query["code"].ToString();
                var slackState = Request.Query["state"].ToString();
                var cookieValue = Request.Cookies[Constants.StanLeeSlackStateCookieName];

                if (slackState != null && cookieValue != null && slackState == cookieValue)
                {
                    SlackAuthRequest slackAuthRequest;
                    string responseMessage;

                    var requestUrl = $"https://slack.com/api/oauth.access?client_id={clientId}&client_secret={clientSecret}&code={slackCode}";
                    var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                    using (var client = new HttpClient())
                    {
                        var response = await client.SendAsync(request).ConfigureAwait(false);
                        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        slackAuthRequest = JsonConvert.DeserializeObject<SlackAuthRequest>(result);
                    }

                    if (slackAuthRequest != null && !slackAuthRequest.IncomingWebhook.Url.IsNullOrWhiteSpace())
                    {
                        _logger.LogInformation("New installation of StanLeeBot for {TeamName} in {Channel}", slackAuthRequest.TeamName, slackAuthRequest.IncomingWebhook.Channel);

                        var webhookUrl = slackAuthRequest.IncomingWebhook.Url;

                        var sbmClient = new SbmClient(webhookUrl);
                        var message = new Message
                        {
                            Text = "Hi there from StanLeeBot! Checkout what I can do by typing /stanlee help"
                        };
                        await sbmClient.SendAsync(message).ConfigureAwait(false);

                        Response.Cookies.Delete(Constants.StanLeeSlackStateCookieName);

                        responseMessage = $"Congrats! StanLeeBot has been successfully added to {slackAuthRequest.TeamName} {slackAuthRequest.IncomingWebhook.Channel}";
                        return RedirectToPage("/Index", new { message = responseMessage });
                    }

                    Response.Cookies.Delete(Constants.StanLeeSlackStateCookieName);

                    _logger.LogError("Something went wrong making a request to {RequestUrl}", requestUrl);

                    responseMessage = "Error: Something went wrong and we were unable to add StanLeeBot to your Slack.";
                    return RedirectToPage("/Index", new { message = responseMessage });
                }

                throw new Exception("State values and Cookie Values do not match.");
            }
            catch (Exception ex)
            {
                Response.Cookies.Delete(Constants.StanLeeSlackStateCookieName);
                
                _logger.LogError(ex, "There was an error with the signin-slack method");
                var responseMessage = "Error: Something went wrong and we were unable to add StanLeeBot to your Slack.";
                return RedirectToPage("/Index", new { message = responseMessage });
            }
        }

        [HttpGet("~/slack-direct")]
        public IActionResult SlackDirectInstall()
        {
            var slackState = Guid.NewGuid().ToString("N");
            Response.Cookies.Append(Constants.StanLeeSlackStateCookieName, slackState);

            var url = $"https://slack.com/oauth/authorize?client_id={_appSettings.Slack.ClientId}&scope={_appSettings.Slack.Scopes}&state={slackState}";

            
            return Redirect(url);
        }

        [HttpGet("~/logout"), HttpPost("~/logout")]
        public IActionResult SignOut()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}