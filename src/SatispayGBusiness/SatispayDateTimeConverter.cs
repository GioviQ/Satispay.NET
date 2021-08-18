using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SatispayGBusiness
{
    public class SatispayDateTimeConverter : JsonConverter<DateTime?>
    {
        private static readonly string _format = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString(), _format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToUniversalTime().ToString(_format));
            else
                writer.WriteNullValue();
        }
    }
}
