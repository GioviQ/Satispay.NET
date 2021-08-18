namespace SatispayGBusiness.Models
{
    public class CreatePaymentResponse<T> : PaymentResponse<T>
    {
        public string QrCodeUrl { get; set; }
    }
}
