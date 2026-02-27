namespace NewsApp.Infrastructure
{
    /// <summary>
    /// Strongly-typed configuration bound from appsettings "NewsApi" section.
    /// </summary>
    public class NewsApiSettings
    {
        public const string SectionName = "NewsApi";

        public string BaseUrl { get; set; } = "https://newsapi.org/v2/";
        public string ApiKey { get; set; } = string.Empty;
        public int CacheDurationMinutes { get; set; } = 5;
        public int DefaultPageSize { get; set; } = 12;
        public int MaxPageSize { get; set; } = 100;
        public int TimeoutSeconds { get; set; } = 15;
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
    }

    /// <summary>
    /// App-level defaults.
    /// </summary>
    public class AppSettings
    {
        public const string SectionName = "App";

        public string DefaultCategory { get; set; } = "general";
        public string DefaultCountry { get; set; } = "us";
        public string DefaultLanguage { get; set; } = "en";
    }
}
