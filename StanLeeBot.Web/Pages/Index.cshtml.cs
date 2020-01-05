using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StanLeeBot.Web.Models;

namespace StanLeeBot.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string SlackState { get; set; }

        public IActionResult OnGet()
        {
            var slackState = Guid.NewGuid().ToString("N");
            SlackState = slackState;
            Response.Cookies.Append(Constants.StanLeeSlackStateCookieName, slackState);

            return Page();
        }
    }
}
