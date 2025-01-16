using Newtonsoft.Json;

namespace Alpha.Models
{
    public class DatasetResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<Dataset>? Results { get; set; }
    }

    public class Dataset
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("category")]
        public Category? Category { get; set; }

        [JsonProperty("subcategory")]
        public Subcategory? Subcategory { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("universe")]
        public string? Universe { get; set; }

        [JsonProperty("coverage")]
        public double Coverage { get; set; }

        [JsonProperty("valueScore")]
        public double ValueScore { get; set; }

        [JsonProperty("userCount")]
        public int UserCount { get; set; }

        [JsonProperty("alphaCount")]
        public int AlphaCount { get; set; }

        [JsonProperty("fieldCount")]
        public int FieldCount { get; set; }

        [JsonProperty("themes")]
        public List<object>? Themes { get; set; }

        [JsonProperty("researchPapers")]
        public List<ResearchPaper>? ResearchPapers { get; set; }
    }

    public class Category
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class Subcategory
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class ResearchPaper
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}
