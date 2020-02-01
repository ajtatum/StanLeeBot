using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly ILogger<UrlShorteningService> _logger;
        private readonly AppSettings _appSettings;

        public UrlShorteningService(ILogger<UrlShorteningService> logger, IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
        }

        public async Task<string> Shorten(string longUrl, string domain, string emailAddress, UrlShorteningServices urlShorteningService, string sessionId)
        {
            var urlIsValid = longUrl.IsValidUrl();
            var emailIsValid = new EmailAddressAttribute().IsValid(emailAddress);
            var authKey = string.Empty;
            var responseMessage = string.Empty;

            authKey = urlShorteningService switch
            {
                UrlShorteningServices.DialogFlow => _appSettings.BabouAuthKeys.DialogFlow,
                UrlShorteningServices.Facebook => _appSettings.BabouAuthKeys.Facebook,
                UrlShorteningServices.Slack => _appSettings.BabouAuthKeys.Slack,
                _ => authKey
            };

            if (domain.Contains("."))
                domain = domain.Replace(".", string.Empty);

            _logger.LogInformation("UrlShorteningService: Beginning to shorten url {LongUrl} using {Domain} using {Service}. SessionId: {SessionId}", longUrl, domain, urlShorteningService, sessionId);

            if (urlIsValid && emailIsValid)
            {
                var restClient = new RestClient(_appSettings.UrlShortenerEndpoint);
                var restRequest = new RestRequest(Method.POST);
                restRequest.AddHeader("AuthKey", authKey);
                restRequest.AddHeader("longUrl", longUrl);
                restRequest.AddHeader("domain", domain);
                restRequest.AddHeader("emailAddress", emailAddress);
                var restResponse = await restClient.ExecuteTaskAsync(restRequest);

                if (!restResponse.ErrorMessage.IsNullOrWhiteSpace())
                {
                    _logger.LogError("UrlShorteningService: There was an error processing the SessionId: {SessionId}. Error message: {ErrorMessage}", sessionId, restResponse.ErrorMessage);

                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"There was an error while processing your request. The error message is {restResponse.ErrorMessage}.");
                    stringBuilder.AppendLine("If you continue to receive this error, please contact us at https://stanleebot.com/Support.");

                    responseMessage = stringBuilder.ToString();
                }
                else
                {
                    _logger.LogInformation("UrlShorteningService: Successfully processed SessionId: {SessionId}", sessionId);

                    var shortUrl = restResponse.Content;

                    responseMessage = $"Excelsior! I've shortened {longUrl} to {shortUrl} and remember your babou.io account is under {emailAddress}.";
                }
            }
            else if (!urlIsValid)
            {
                _logger.LogError("UrlShorteningService: Long URL is invalid for SessionId: {SessionId}", sessionId);

                responseMessage = $"The url you provided, {longUrl}, doesn't seem to be a valid URL. Please try again.";
            }
            else if (!emailIsValid)
            {
                _logger.LogError("UrlShorteningService: Email is invalid for SessionId: {SessionId}", sessionId);

                responseMessage = $"The email address you provided, {emailAddress}, doesn't seem to be a valid email. Please try again.";
            }

            return responseMessage;
        }
    }
}
