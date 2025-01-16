using Alpha.Converters;
using Newtonsoft.Json;

namespace Alpha.Models
{
    public class WorldQuantAccountLoginResponse
    {
        public User? User { get; set; }
        public Token? Token { get; set; }
        public List<string> Permissions { get; set; }

        public WorldQuantAccountLoginResponse()
        {
            Permissions = [];
        }
    }

    public class User
    {
        public string? Id { get; set; }
    }

    public class Token
    {
        [JsonConverter(typeof(ExpiryToDateTimeConverter))]
        public DateTime Expiry { get; set; }
    }
}
