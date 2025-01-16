using Newtonsoft.Json;

namespace Alpha.Models
{
    public class SimulationSettings
    {
        [JsonProperty("instrumentType")]
        public string? InstrumentType { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("universe")]
        public string? Universe { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("decay")]
        public int Decay { get; set; }

        [JsonProperty("neutralization")]
        public string? Neutralization { get; set; }

        [JsonProperty("truncation")]
        public double Truncation { get; set; }

        [JsonProperty("pasteurization")]
        public string? Pasteurization { get; set; }

        [JsonProperty("unitHandling")]
        public string? UnitHandling { get; set; }

        [JsonProperty("nanHandling")]
        public string? NanHandling { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("visualization")]
        public bool Visualization { get; set; }
    }

    public class SimulationData
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("regular")]
        public string? Regular { get; set; }

        [JsonProperty("settings")]
        public SimulationSettings? Settings { get; set; }

        public static SimulationData CreateDefault()
        {
            return new SimulationData
            {
                Type = "REGULAR",
                Regular = "liabilities/assets",
                Settings = new SimulationSettings
                {
                    InstrumentType = "EQUITY",
                    Region = "USA",
                    Universe = "TOP3000",
                    Delay = 1,
                    Decay = 0,
                    Neutralization = "INDUSTRY",
                    Truncation = 0.08,
                    Pasteurization = "ON",
                    UnitHandling = "VERIFY",
                    NanHandling = "OFF",
                    Language = "FASTEXPR",
                    Visualization = false
                }
            };
        }
    }
}
