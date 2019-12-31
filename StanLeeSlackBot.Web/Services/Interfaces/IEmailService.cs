using System.Threading.Tasks;

namespace StanLeeSlackBot.Web.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendSupportMessage(string fromEmail, string fromName, string subject, string htmlMessage);
    }
}
