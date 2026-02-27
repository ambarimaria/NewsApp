using Newtonsoft.Json;

namespace NewsApp.Models.Api
{
    // ── Top-level API responses ───────────────────────────────────────────────

    public class NewsApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("code")]
        public string? Code { get; set; }           // present on errors

        [JsonProperty("message")]
        public string? Message { get; set; }        // present on errors

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }

        [JsonProperty("articles")]
        public List<ArticleDto> Articles { get; set; } = new();

        public bool IsSuccess => Status == "ok";
    }

    public class SourcesApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("sources")]
        public List<SourceDto> Sources { get; set; } = new();

        public bool IsSuccess => Status == "ok";
    }

    // ── Article ───────────────────────────────────────────────────────────────

    public class ArticleDto
    {
        [JsonProperty("source")]
        public ArticleSourceDto? Source { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("urlToImage")]
        public string? UrlToImage { get; set; }

        [JsonProperty("publishedAt")]
        public DateTime? PublishedAt { get; set; }

        [JsonProperty("content")]
        public string? Content { get; set; }
    }

    public class ArticleSourceDto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }

    // ── Source ────────────────────────────────────────────────────────────────

    public class SourceDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }
    }
}
