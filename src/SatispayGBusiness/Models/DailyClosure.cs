using System;
using System.Text.Json.Serialization;

namespace SatispayGBusiness.Models
{
    public class DailyClosure
    {
        public string id { get; set; }

        [JsonConverter(typeof(SatispayDateTimeConverter))]
        public DateTime? date { get; set; }
    }
}
