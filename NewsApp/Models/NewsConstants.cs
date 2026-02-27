namespace NewsApp.Models
{
    public static class NewsConstants
    {
        public static readonly List<string> Categories = new()
        {
            "general", "business", "entertainment", "health",
            "science", "sports", "technology"
        };

        public static readonly List<KeyValuePair<string, string>> Countries = new()
        {
            new("us", "United States"),
            new("gb", "United Kingdom"),
            new("au", "Australia"),
            new("ca", "Canada"),
            new("in", "India"),
            new("de", "Germany"),
            new("fr", "France"),
            new("it", "Italy"),
            new("jp", "Japan"),
            new("br", "Brazil"),
            new("mx", "Mexico"),
            new("za", "South Africa"),
            new("ae", "UAE"),
            new("sg", "Singapore"),
            new("nz", "New Zealand"),
        };

        public static readonly List<KeyValuePair<string, string>> SortOptions = new()
        {
            new("publishedAt", "Newest First"),
            new("relevancy",   "Most Relevant"),
            new("popularity",  "Most Popular"),
        };

        public static readonly List<KeyValuePair<string, string>> Languages = new()
        {
            new("en", "English"),
            new("ar", "Arabic"),
            new("de", "German"),
            new("es", "Spanish"),
            new("fr", "French"),
            new("he", "Hebrew"),
            new("it", "Italian"),
            new("nl", "Dutch"),
            new("no", "Norwegian"),
            new("pt", "Portuguese"),
            new("ru", "Russian"),
            new("sv", "Swedish"),
            new("zh", "Chinese"),
        };

        public static readonly Dictionary<string, string> CategoryIcons = new()
        {
            { "general",       "fa-newspaper" },
            { "business",      "fa-briefcase" },
            { "entertainment", "fa-film" },
            { "health",        "fa-heart-pulse" },
            { "science",       "fa-flask" },
            { "sports",        "fa-trophy" },
            { "technology",    "fa-microchip" },
        };

        public static string GetCategoryIcon(string category)
            => CategoryIcons.TryGetValue(category.ToLower(), out var icon) ? icon : "fa-newspaper";

        /// <summary>
        /// Maps country codes to their primary language for the "everything" fallback search.
        /// Used when top-headlines returns no results for that country.
        /// </summary>
        public static readonly Dictionary<string, string> CountryLanguage = new()
        {
            { "us", "en" }, { "gb", "en" }, { "au", "en" }, { "ca", "en" },
            { "nz", "en" }, { "sg", "en" }, { "za", "en" },
            { "in", "en" },
            { "de", "de" }, { "at", "de" },
            { "fr", "fr" },
            { "it", "it" },
            { "br", "pt" }, { "pt", "pt" },
            { "mx", "es" }, { "ar", "es" },
            { "jp", "ja" },
            { "ae", "ar" }, { "sa", "ar" },
            { "ru", "ru" },
            { "cn", "zh" },
        };

        /// <summary>
        /// Maps country codes to well-known NewsAPI source IDs.
        /// These are used to pass as "sources" param when top-headlines returns
        /// empty/wrong results for a given country.
        /// </summary>
        public static readonly Dictionary<string, string[]> CountrySources = new()
        {
            { "us", new[] { "associated-press", "reuters", "the-washington-post",
                            "the-new-york-times", "cnn", "fox-news", "usa-today",
                            "abc-news", "nbc-news", "cbs-news", "axios", "politico" } },
            { "gb", new[] { "bbc-news", "the-guardian-uk", "the-telegraph",
                            "independent", "mirror", "the-sun", "sky-news",
                            "the-times", "financial-times" } },
            { "au", new[] { "abc-news-au", "australian-financial-review",
                            "news-com-au", "the-sydney-morning-herald" } },
            { "ca", new[] { "cbc-news", "financial-post", "the-globe-and-mail",
                            "national-post" } },
            { "in", new[] { "the-times-of-india", "the-hindu", "india-today",
                            "ndtv", "economic-times" } },
            { "de", new[] { "der-tagesspiegel", "die-zeit", "focus",
                            "handelsblatt", "spiegel-online", "t3n", "wired-de" } },
            { "fr", new[] { "le-monde", "liberation", "les-echos" } },
            { "it", new[] { "ansa", "il-sole-24-ore", "la-repubblica" } },
            { "jp", new[] { "asahi-shimbun" } },
            { "br", new[] { "globo", "ig-news", "infodinero" } },
            { "mx", new[] { "la-jornada", "proceso" } },
            { "za", new[] { "news24", "the-citizen-za" } },
            { "ae", new[] { "the-national" } },
            { "sg", new[] { "channel-news-asia" } },
            { "nz", new[] { "news-com-au" } },  // closest available
        };

        /// <summary>
        /// Returns comma-separated source IDs for a country code (max 20, NewsAPI limit).
        /// Returns null if country not mapped (fall back to country param).
        /// </summary>
        public static string? GetSourcesForCountry(string countryCode)
        {
            if (CountrySources.TryGetValue(countryCode.ToLower(), out var sources) && sources.Length > 0)
                return string.Join(",", sources.Take(20));
            return null;
        }

        /// <summary>Returns the dominant language for a country code.</summary>
        public static string GetLanguageForCountry(string countryCode)
            => CountryLanguage.TryGetValue(countryCode.ToLower(), out var lang) ? lang : "en";

    }
}
