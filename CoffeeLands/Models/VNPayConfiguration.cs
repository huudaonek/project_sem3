namespace CoffeeLands.Models
{
    public class VNPayConfiguration
    {
        private readonly IConfiguration _config;

        public VNPayConfiguration(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContent content, VnPaymentRequestModel model)
        {
            // Implement the logic for CreatePaymentUrl
            throw new NotImplementedException();
        }

        public VnPaymentResponseModel PaymentExcute(IQueryCollection collections)
        {
            // Implement the logic for PaymentExcute
            throw new NotImplementedException();
        }
    }
}
