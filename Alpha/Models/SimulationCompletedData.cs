using Newtonsoft.Json;

namespace Alpha.Models
{
    public class SimulationCompletedSettings
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

    public class SimulationCompletedData
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("regular")]
        public string? Regular { get; set; }

        public string? Time { get; set; }

        [JsonProperty("settings")]
        public SimulationCompletedSettings? Settings { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("location")]
        public SimulationCompletedLocation? Location { get; set; }

        [JsonProperty("alpha")]
        public string? Alpha { get; set; }

        public AlphaResponse? AlphaResult { get; set; }

        public static SimulationCompletedData CreateDefault()
        {
            return new SimulationCompletedData
            {
                Type = "REGULAR",
                Regular = "liabilities/assets",
                Settings = new SimulationCompletedSettings
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

    public class SimulationCompletedLocation
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("property")]
        public string? Property { get; set; }
    }
}
