using System.Text.Json.Serialization;

namespace SatispayGBusiness.Models
{
    public class Sender
    {
        public string id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActorType type { get; set; } = ActorType.CONSUMER;
        public string name { get; set; }
    }
}
