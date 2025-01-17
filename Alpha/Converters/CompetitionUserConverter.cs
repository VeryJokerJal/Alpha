using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alpha.Converters
{
    public class CompetitionUserConverter : JsonConverter<string>
    {
        public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                // 处理简单字符串形式
                return reader.Value?.ToString();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // 处理复杂对象形式
                JObject jObject = JObject.Load(reader);
                JToken? name = jObject["id"];
                return name?.Value<string>();
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                // 如果是简单字符串，直接写出
                writer.WriteValue(value);
            }
        }
    }
}