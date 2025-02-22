﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Alpha.Models
{
    public class DatasetIdResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<DatasetIdResult>? Results { get; set; }
    }

    public class DatasetIdResult
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("dataset")]
        public Dataset? Dataset { get; set; }

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

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("coverage")]
        public double Coverage { get; set; }

        [JsonProperty("userCount")]
        public int UserCount { get; set; }

        [JsonProperty("alphaCount")]
        public int AlphaCount { get; set; }

        [JsonProperty("themes")]
        public List<object>? Themes { get; set; }
    }
}
