using CoffeeLands.ViewModels.Mail;

namespace CoffeeLands.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task SendBodyEmailAsync(SendMailRequest request);
    }
}
