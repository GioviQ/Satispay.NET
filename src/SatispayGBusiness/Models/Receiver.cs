using System.Text.Json.Serialization;

namespace SatispayGBusiness.Models
{
    public class Receiver
    {
        public string id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActorType type { get; set; } = ActorType.SHOP;
    }
}
