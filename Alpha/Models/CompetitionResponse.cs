using Alpha.Converters;
using Newtonsoft.Json;

namespace Alpha.Models
{
    public class CompetitionResponse
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("universities")]
        public object? Universities { get; set; }

        [JsonProperty("countries")]
        public object? Countries { get; set; }

        [JsonProperty("excludedCountries")]
        public object? ExcludedCountries { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("teamBased")]
        public bool TeamBased { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("signUpStartDate")]
        public DateTime? SignUpStartDate { get; set; }

        [JsonProperty("signUpEndDate")]
        public DateTime? SignUpEndDate { get; set; }

        [JsonProperty("signUpDate")]
        public DateTime SignUpDate { get; set; }

        [JsonProperty("team")]
        public object? Team { get; set; }

        [JsonProperty("scoring")]
        public string? Scoring { get; set; }

        [JsonProperty("leaderboard")]
        public CompetitionLeaderboard? Leaderboard { get; set; }

        [JsonProperty("prizeBoard")]
        public bool PrizeBoard { get; set; }

        [JsonProperty("universityBoard")]
        public bool UniversityBoard { get; set; }

        [JsonProperty("submissions")]
        public bool Submissions { get; set; }

        [JsonProperty("faq")]
        public string? Faq { get; set; }

        [JsonProperty("progress")]
        public CompetitionProgress? Progress { get; set; }
    }

    public class CompetitionLeaderboard
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("user")]
        [JsonConverter(typeof(CompetitionUserConverter))]
        public string? User { get; set; }

        [JsonProperty("level")]
        public string? Level { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("alphas")]
        public int Alphas { get; set; }

        [JsonProperty("university")]
        public object? University { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }
    }

    public class CompetitionProgress
    {
        [JsonProperty("level")]
        public string? Level { get; set; }

        [JsonProperty("score")]
        public CompetitionScoreProgress? Score { get; set; }
    }

    public class CompetitionScoreProgress
    {
        [JsonProperty("bottom")]
        public double Bottom { get; set; }

        [JsonProperty("top")]
        public double Top { get; set; }

        [JsonProperty("remaining")]
        public double Remaining { get; set; }
    }
}
