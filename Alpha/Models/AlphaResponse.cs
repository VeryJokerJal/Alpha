using Newtonsoft.Json;

namespace Alpha.Models
{
    public class AlphaResponse
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("settings")]
        public AlphaSettings? Settings { get; set; }

        [JsonProperty("regular")]
        public AlphaRegular? Regular { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("dateSubmitted")]
        public DateTime? DateSubmitted { get; set; }

        [JsonProperty("dateModified")]
        public DateTime DateModified { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }

        [JsonProperty("color")]
        public string? Color { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("tags")]
        public List<string>? Tags { get; set; }

        [JsonProperty("classifications")]
        public List<AlphaClassification>? Classifications { get; set; }

        [JsonProperty("grade")]
        public string? Grade { get; set; }

        [JsonProperty("stage")]
        public string? Stage { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("is")]
        public AlphaPerformanceMetrics? Is { get; set; }

        [JsonProperty("os")]
        public object? Os { get; set; }

        [JsonProperty("train")]
        public AlphaPerformanceMetrics? Train { get; set; }

        [JsonProperty("test")]
        public AlphaPerformanceMetrics? Test { get; set; }

        [JsonProperty("prod")]
        public object? Prod { get; set; }

        [JsonProperty("competitions")]
        public object? Competitions { get; set; }

        [JsonProperty("themes")]
        public object? Themes { get; set; }

        [JsonProperty("pyramids")]
        public object? Pyramids { get; set; }

        [JsonProperty("team")]
        public object? Team { get; set; }
    }

    public class AlphaSettings
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

        [JsonProperty("testPeriod")]
        public string? TestPeriod { get; set; }
    }

    public class AlphaRegular
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("operatorCount")]
        public int OperatorCount { get; set; }
    }

    public class AlphaClassification
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class AlphaPerformanceMetrics
    {
        [JsonProperty("pnl")]
        public int Pnl { get; set; }

        [JsonProperty("bookSize")]
        public int BookSize { get; set; }

        [JsonProperty("longCount")]
        public int LongCount { get; set; }

        [JsonProperty("shortCount")]
        public int ShortCount { get; set; }

        [JsonProperty("turnover")]
        public double Turnover { get; set; }

        [JsonProperty("returns")]
        public double Returns { get; set; }

        [JsonProperty("drawdown")]
        public double Drawdown { get; set; }

        [JsonProperty("margin")]
        public double Margin { get; set; }

        [JsonProperty("sharpe")]
        public double Sharpe { get; set; }

        [JsonProperty("fitness")]
        public double Fitness { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("checks")]
        public List<AlphaCheck>? Checks { get; set; }
    }

    public class AlphaCheck
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("result")]
        public string? Result { get; set; }

        [JsonProperty("limit")]
        public double? Limit { get; set; }

        [JsonProperty("value")]
        public double? Value { get; set; }

        [JsonProperty("competitions")]
        public List<AlphaCompetition>? Competitions { get; set; }
    }

    public class AlphaCompetition
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}
