using System;
using System.Text.Json.Serialization;

namespace SatispayGBusiness.Models
{
    //https://developers.satispay.com/reference#create-a-payment
    public class PaymentResponse<T>
    {
        public string id { get; set; }

        //QR Code
        public string code_identifier { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentType type { get; set; }
        public int amount_unit { get; set; }
        public string currency { get; set; } = "EUR";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status status { get; set; }
        public bool expired { get; set; }

        //Metadata inserted within the payment request
        public T metadata { get; set; }
        public Sender sender { get; set; }
        public Receiver receiver { get; set; }

        [JsonConverter(typeof(SatispayDateTimeConverter))]
        public DateTime? insert_date { get; set; }

        [JsonConverter(typeof(SatispayDateTimeConverter))]
        public DateTime? expire_date { get; set; }

        //Order ID or payment external identifier
        public string external_code { get; set; }

        //https://online.satispay.com/pay/41da7b74-a9f4-4d25-8428-0e3e460d90c1?redirect_url=https%3A%2F%2FmyServer.com%2FmyRedirectUrl
        public string redirect_url { get; set; }
    }
}
