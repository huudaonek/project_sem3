using CoffeeLands.Models;

namespace CoffeeLands.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task SendWelcomeEmailAsync(WelcomeRequest request);
        Task SendThankYouEmailAsync(ThankYouRequest request);
    }
}
