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

        //For PRE_AUTHORIZED Flow
        public string pre_authorized_payments_token { get; set; }

        //For REFUND Flow
        public string parent_payment_uid { get; set; }
        public string currency { get; set; } = "EUR";

        [JsonConverter(typeof(SatispayDateTimeConverter))]
        public DateTime? expiration_date { get; set; }

        //Order ID or payment external identifier
        public string external_code { get; set; }

        //https://myServer.com/myCallbackUrl?payment_id={uuid}
        public string callback_url { get; set; }

        //For MATCH_CODE Flow
        //https://myServer.com/myRedirectUrl
        public string redirect_url { get; set; }
        public T metadata { get; set; }

        //For MATCH_USER Flow
        public string consumer_uid { get; set; }
    }

    public class CreatePaymentRequest : CreatePaymentRequest<object>
    {

    }
}
