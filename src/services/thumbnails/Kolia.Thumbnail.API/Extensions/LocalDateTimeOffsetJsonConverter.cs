using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kolia.Thumbnail.API.Extensions
{
    public class LocalDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        private readonly string _format;

        public LocalDateTimeOffsetJsonConverter(string format = "yyyy-MM-dd HH:mm:ss zzz")
        {
            _format = format;
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && DateTimeOffset.TryParse(reader.GetString(), out var date))
            {
                return date;
            }
            return reader.GetDateTimeOffset();
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToLocalTime().ToString(_format));
        }
    }
}
