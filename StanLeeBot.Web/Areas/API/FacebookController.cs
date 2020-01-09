using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Grpc.Auth;
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
            // Backup method in case Azure is unhappy
            //var cred = GoogleCredential.FromFile(_appSettings.DialogFlow.CredentialsFilePath);
            //var channel = new Channel(SessionsClient.DefaultEndpoint.Host, 
            //    SessionsClient.DefaultEndpoint.Port, cred.ToChannelCredentials());

            var sessionId = Guid.NewGuid().ToString("N");
            var sessionsClient = await SessionsClient.CreateAsync();

            var requestIntent = new DetectIntentRequest
            {
                SessionAsSessionName = new SessionName(_appSettings.DialogFlow.ProjectId, sessionId),
                QueryInput = new QueryInput()
                {
                    Text = new TextInput()
                    {
                        Text = "Hi",
                        LanguageCode = "en-US"
                    }
                }
            };

            var responseIntent = await sessionsClient.DetectIntentAsync(requestIntent);

            var responseText = responseIntent.QueryResult?.FulfillmentText ?? "Sorry, I didn't understand.";

            return new OkObjectResult(responseText);
        }
    }
}