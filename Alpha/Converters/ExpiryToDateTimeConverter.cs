using Newtonsoft.Json;

namespace Alpha.Converters
{
    public class ExpiryToDateTimeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                writer.WriteValue(((DateTime)value - new DateTime(1970, 1, 1)).TotalSeconds);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            if ((uint)(reader.TokenType - 7) <= 1u)
            {
                double totalSeconds;
                totalSeconds = Convert.ToDouble(reader.Value);
                return DateTime.Now.AddSeconds(totalSeconds);
            }
            throw new JsonSerializationException("Unexpected token type: " + reader.TokenType);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }
    }
}
