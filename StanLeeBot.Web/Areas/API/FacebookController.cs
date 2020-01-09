using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Cloud.Dialogflow.V2;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Areas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacebookController : ControllerBase
    {
        private readonly ILogger<FacebookController> _logger;
        private readonly AppSettings _appSettings;

        public FacebookController(ILogger<FacebookController> logger, IOptionsMonitor<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.CurrentValue;
        }

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "hub.mode")] string mode, [FromQuery(Name = "hub.challenge")] string challenge, [FromQuery(Name = "hub.verify_token")] string verify_token)
        {
            if (mode == "subscribe" && verify_token == _appSettings.Facebook.VerificationToken)
            {
                return new OkObjectResult(challenge);
            }

            return new NotFoundResult();
        }

        [HttpPost]
        public IActionResult Post()
        {
            try
            {
                var json = (dynamic)null;
                using (StreamReader sr = new StreamReader(this.Request.Body))
                {
                    json = sr.ReadToEnd();
                }
                dynamic data = JsonConvert.DeserializeObject(json);
                //postToFB((string)data.entry[0].messaging[0].sender.id, (string)data.entry[0].messaging[0].message.text);

                return new OkResult();
            }
            catch (Exception ex)
            {
                return new NotFoundResult();
            }
        }

        [HttpGet("dialogflow")]
        public async Task<IActionResult> GetAiResponse()
        {
            var serviceEndpoint = new ServiceEndpoint($"https://{SessionsClient.DefaultEndpoint.Host}?key={_appSettings.DialogFlow.ApiKey}");

            var client = await SessionsClient.CreateAsync();

            var responseText = string.Empty;
            var texts = new[] {"Hi", "Hey"};
            foreach (var text in texts)
            {
                var response = await client.DetectIntentAsync(
                    session: new SessionName("", "123456"),
                    queryInput: new QueryInput()
                    {
                        Text = new TextInput()
                        {
                            Text = text,
                            LanguageCode = "en-US"
                        }
                    });

                var queryResult = response.QueryResult;

                if (queryResult.Intent != null)
                {
                    responseText = queryResult.FulfillmentText;
                }
            }
            return new OkObjectResult(responseText);
        }
    }
}