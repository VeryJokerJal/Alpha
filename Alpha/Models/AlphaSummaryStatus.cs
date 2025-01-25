using Newtonsoft.Json;

namespace Alpha.Models
{
    public class AlphaSummaryStatus
    {
        [JsonProperty("unsubmitted")]
        public int Unsubmitted { get; set; }

        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("decommissioned")]
        public int Decommissioned { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
