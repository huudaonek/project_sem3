using CoffeeLands.Models;

namespace CoffeeLands.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExcute(IQueryCollection collections);
    }
}
