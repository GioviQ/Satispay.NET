using System;
using System.Text.Json.Serialization;

namespace SatispayGBusiness.Models
{
    //https://developers.satispay.com/reference#create-a-payment
    public class CreatePaymentRequest<T>
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Flow flow { get; set; }
        public int amount_unit { get; set; }
        public string pre_authorized_payments_token { get; set; }
        public string parent_payment_uid { get; set; }
        public string currency { get; set; } = "EUR";

        [JsonConverter(typeof(SatispayDateTimeConverter))]
        public DateTime? expiration_date { get; set; }

        //Order ID or payment external identifier
        public string external_code { get; set; }

        //https://myServer.com/myCallbackUrl?payment_id={uuid}
        public string callback_url { get; set; }
        public T metadata { get; set; }
        public string consumer_uid { get; set; }

        public string redirect_url { get; set; } = "";
    }

    public class CreatePaymentRequest : CreatePaymentRequest<object>
    {

    }
}
