using CoffeeLands.Models;
using CoffeeLands.ViewModels.Momo;


namespace CoffeeLands.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
