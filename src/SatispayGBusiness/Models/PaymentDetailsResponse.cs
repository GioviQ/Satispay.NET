namespace SatispayGBusiness.Models
{
    public class PaymentDetailsResponse<T> : PaymentResponse<T>
    {
        public DailyClosure daily_closure { get; set; }
    }
}
