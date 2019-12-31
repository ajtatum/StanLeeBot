using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StanLeeSlackBot.Web.Helpers.Attributes;
using StanLeeSlackBot.Web.Services.Interfaces;

namespace StanLeeSlackBot.Web.Pages
{
    public class SupportModel : PageModel
    {
        private readonly IEmailService _emailService;

        public SupportModel(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please provide your email address.")]
            [EmailAddress]
            [Display(Name = "Your Email")]
            public string FromEmail { get; set; }

            [Required(ErrorMessage = "Please provide your name.")]
            [Display(Name = "Your Name")]
            [NoHtml(ErrorMessage = "HTML is not allowed in the name field.")]
            public string FromName { get; set; }

            [Required]
            [NoHtml(ErrorMessage = "HTML is not allowed in the subject field.")]
            public string Subject { get; set; }

            [Required(ErrorMessage = "Please tell us why you're contacting us.")]
            [NoHtml(ErrorMessage = "HTML is not allowed in the message field.")]
            public string Message { get; set; }
        }

        public IActionResult OnGet()
        {
            Input = new InputModel();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _emailService.SendSupportMessage(Input.FromEmail, Input.FromName, Input.Subject, Input.Message);

            StatusMessage = "Your message has been sent!";
            return RedirectToPage();
        }
    }
}