using Microsoft.Extensions.Caching.Memory;
using NewsApp.Infrastructure;
using NewsApp.Infrastructure.Cache;
using NewsApp.Infrastructure.Exceptions;
using NewsApp.Models;
using NewsApp.Models.Api;
using NewsApp.Models.ViewModels;
using NewsApp.Services.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Web;

namespace NewsApp.Services
{
    /// <summary>
    /// Encapsulates all communication with the NewsAPI.
    /// Features:
    ///  - Strongly-typed options via NewsApiSettings
    ///  - IMemoryCache with deterministic keys
    ///  - Polly retry + circuit-breaker (wired in DI)
    ///  - Structured exception mapping (rate-limit, invalid key, etc.)
    ///  - Full ILogger integration
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly NewsApiSettings _settings;
        private readonly ILogger<NewsService> _logger;

        public NewsService(
            IHttpClientFactory factory,
            IMemoryCache cache,
            NewsApiSettings settings,
            ILogger<NewsService> logger)
        {
            _http = factory.CreateClient("NewsApi");
            _cache = cache;
            _settings = settings;
            _logger = logger;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public async Task<(List<Article> Articles, int TotalResults, bool FromCache)>
            GetTopHeadlinesAsync(TopHeadlinesQuery q)
        {
            // NewsAPI: can't mix "sources" with "country"/"category"
            bool hasSources = !string.IsNullOrWhiteSpace(q.Sources);
            string country = hasSources ? "" : (q.Country ?? "us");
            string category = hasSources ? "" : (q.Category ?? "general");

            var cacheKey = CacheKeyBuilder.TopHeadlines(country, category, q.Sources, q.Query, q.Page, q.PageSize);

            if (_cache.TryGetValue(cacheKey, out (List<Article>, int) cached))
            {
                _logger.LogDebug("Cache HIT: {Key}", cacheKey);
                return (cached.Item1, cached.Item2, true);
            }

            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (hasSources) qs["sources"] = q.Sources;
            else
            {
                if (!string.IsNullOrEmpty(country)) qs["country"] = country;
                if (!string.IsNullOrEmpty(category)) qs["category"] = category;
            }
            if (!string.IsNullOrWhiteSpace(q.Query)) qs["q"] = q.Query;
            qs["page"] = q.Page.ToString();
            qs["pageSize"] = q.PageSize.ToString();

            var response = await FetchAsync<NewsApiResponse>($"top-headlines?{qs}");
            var articles = FilterArticles(response.Articles);

            _cache.Set(cacheKey, (articles, response.TotalResults),
                TimeSpan.FromMinutes(_settings.CacheDurationMinutes));

            return (articles, response.TotalResults, false);
        }

        public async Task<(List<Article> Articles, int TotalResults, bool FromCache)>
            SearchEverythingAsync(NewsSearchQuery q)
        {
            if (string.IsNullOrWhiteSpace(q.Query))
                return (new(), 0, false);

            var cacheKey = CacheKeyBuilder.Everything(
                q.Query, q.Sources, q.Language, q.SortBy, q.From, q.To, q.Page, q.PageSize);

            if (_cache.TryGetValue(cacheKey, out (List<Article>, int) cached))
            {
                _logger.LogDebug("Cache HIT: {Key}", cacheKey);
                return (cached.Item1, cached.Item2, true);
            }

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["q"] = q.Query;
            if (!string.IsNullOrWhiteSpace(q.Sources)) qs["sources"] = q.Sources;
            if (!string.IsNullOrWhiteSpace(q.Language)) qs["language"] = q.Language;
            if (!string.IsNullOrWhiteSpace(q.SortBy)) qs["sortBy"] = q.SortBy;
            if (!string.IsNullOrWhiteSpace(q.From)) qs["from"] = q.From;
            if (!string.IsNullOrWhiteSpace(q.To)) qs["to"] = q.To;
            qs["page"] = q.Page.ToString();
            qs["pageSize"] = q.PageSize.ToString();

            var response = await FetchAsync<NewsApiResponse>($"everything?{qs}");
            var articles = FilterArticles(response.Articles);

            _cache.Set(cacheKey, (articles, response.TotalResults),
                TimeSpan.FromMinutes(_settings.CacheDurationMinutes));

            return (articles, response.TotalResults, false);
        }

        public async Task<(List<SourceDto> Sources, bool FromCache)>
            GetSourcesAsync(string? category = null, string? language = null, string? country = null)
        {
            var cacheKey = CacheKeyBuilder.Sources(category, language, country);

            if (_cache.TryGetValue(cacheKey, out List<SourceDto>? cached) && cached != null)
            {
                _logger.LogDebug("Cache HIT: {Key}", cacheKey);
                return (cached, true);
            }

            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(category)) qs["category"] = category;
            if (!string.IsNullOrWhiteSpace(language)) qs["language"] = language;
            if (!string.IsNullOrWhiteSpace(country)) qs["country"] = country;

            var response = await FetchAsync<SourcesApiResponse>($"top-headlines/sources?{qs}");

            _cache.Set(cacheKey, response.Sources,
                TimeSpan.FromMinutes(_settings.CacheDurationMinutes * 6)); // sources change rarely

            return (response.Sources, false);
        }

        // ── Country-aware headlines (3-tier fallback) ─────────────────────────

        public async Task<(List<Article> Articles, int TotalResults, bool FromCache, string Strategy)>
            GetHeadlinesByCountryAsync(string country, string category, int page, int pageSize)
        {
            country = country.ToLower().Trim();

            // ── Strategy 1: top-headlines with country param ──────────────────
            // Works well for US, GB, AU, CA, IN, DE, FR and other major markets.
            var cacheKey1 = $"country_s1_{country}_{category}_{page}_{pageSize}";
            if (_cache.TryGetValue(cacheKey1, out (List<Article>, int, string) c1))
                return (c1.Item1, c1.Item2, true, c1.Item3);

            try
            {
                var qs1 = HttpUtility.ParseQueryString(string.Empty);
                qs1["country"] = country;
                qs1["category"] = category;
                qs1["page"] = page.ToString();
                qs1["pageSize"] = pageSize.ToString();

                _logger.LogInformation("[Country S1] top-headlines?country={C}&category={Cat}", country, category);
                var r1 = await FetchAsync<NewsApiResponse>($"top-headlines?{qs1}");
                var articles1 = FilterArticles(r1.Articles);

                if (articles1.Count >= 3)
                {
                    var result = (articles1, r1.TotalResults, "top-headlines");
                    _cache.Set(cacheKey1, result, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                    return (articles1, r1.TotalResults, false, "top-headlines");
                }

                _logger.LogInformation("[Country S1] Only {N} results for {C}, trying S2 (sources)", articles1.Count, country);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Country S1] Failed for {C}", country);
            }

            // ── Strategy 2: top-headlines filtered by country-specific sources ─
            // NewsAPI free tier respects "sources" param reliably.
            var mappedSources = NewsConstants.GetSourcesForCountry(country);
            if (!string.IsNullOrEmpty(mappedSources))
            {
                var cacheKey2 = $"country_s2_{country}_{category}_{page}_{pageSize}";
                if (_cache.TryGetValue(cacheKey2, out (List<Article>, int, string) c2))
                    return (c2.Item1, c2.Item2, true, c2.Item3);

                try
                {
                    // NewsAPI: cannot combine "sources" with "category" — we omit category here
                    var qs2 = HttpUtility.ParseQueryString(string.Empty);
                    qs2["sources"] = mappedSources;
                    qs2["page"] = page.ToString();
                    qs2["pageSize"] = pageSize.ToString();

                    _logger.LogInformation("[Country S2] top-headlines?sources={S}", mappedSources[..Math.Min(40, mappedSources.Length)]);
                    var r2 = await FetchAsync<NewsApiResponse>($"top-headlines?{qs2}");
                    var articles2 = FilterArticles(r2.Articles);

                    if (articles2.Count >= 1)
                    {
                        var result = (articles2, r2.TotalResults, "sources");
                        _cache.Set(cacheKey2, result, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                        return (articles2, r2.TotalResults, false, "sources");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Country S2] Failed for {C}", country);
                }
            }

            // ── Strategy 3: everything search using country name + language ────
            // Broadest fallback — searches all articles mentioning the country.
            var cacheKey3 = $"country_s3_{country}_{category}_{page}_{pageSize}";
            if (_cache.TryGetValue(cacheKey3, out (List<Article>, int, string) c3))
                return (c3.Item1, c3.Item2, true, c3.Item3);

            try
            {
                var countryName = NewsConstants.Countries
                    .FirstOrDefault(kv => kv.Key == country).Value ?? country;
                var lang = NewsConstants.GetLanguageForCountry(country);

                // Build a relevant query: use category keyword + country name
                var categoryKeyword = category == "general" ? "news" : category;
                var searchQuery = $"{categoryKeyword} {countryName}";

                var qs3 = HttpUtility.ParseQueryString(string.Empty);
                qs3["q"] = searchQuery;
                qs3["language"] = lang;
                qs3["sortBy"] = "publishedAt";
                qs3["page"] = page.ToString();
                qs3["pageSize"] = pageSize.ToString();

                _logger.LogInformation("[Country S3] everything?q={Q}&language={L}", searchQuery, lang);
                var r3 = await FetchAsync<NewsApiResponse>($"everything?{qs3}");
                var articles3 = FilterArticles(r3.Articles);

                var result3 = (articles3, r3.TotalResults, "search");
                _cache.Set(cacheKey3, result3, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                return (articles3, r3.TotalResults, false, "search");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Country S3] Failed for {C}", country);
                return (new List<Article>(), 0, false, "failed");
            }
        }

        public async Task<List<Article>> GetRelatedArticlesAsync(string title, int count = 6)
        {
            try
            {
                // Extract keywords from title (first 3 meaningful words)
                var keywords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3)
                    .Take(3)
                    .ToList();

                if (!keywords.Any()) return new();

                var query = string.Join(" OR ", keywords);
                var (articles, _, _) = await SearchEverythingAsync(new NewsSearchQuery
                {
                    Query = query,
                    Language = "en",
                    SortBy = "relevancy",
                    Page = 1,
                    PageSize = count + 1
                });

                // Exclude exact title match
                return articles
                    .Where(a => !string.Equals(a.Title, title, StringComparison.OrdinalIgnoreCase))
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch related articles");
                return new();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static List<Article> FilterArticles(IEnumerable<ArticleDto> dtos)
            => dtos
                .Where(a => !string.IsNullOrWhiteSpace(a.Title) && a.Title != "[Removed]")
                .Select(Article.FromDto)
                .ToList();

        // ── Private HTTP Core ─────────────────────────────────────────────────

        private async Task<T> FetchAsync<T>(string relativeUrl) where T : class
        {
            _logger.LogInformation("NewsAPI request: {Url}", relativeUrl);

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _http.GetAsync(relativeUrl);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "NewsAPI request timed out");
                throw new NewsApiException("The request to NewsAPI timed out. Please try again.", inner: ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "NewsAPI connection error");
                throw new NewsApiException("Could not connect to NewsAPI. Check your internet connection.", inner: ex);
            }

            // Map common HTTP errors
            if (!httpResponse.IsSuccessStatusCode)
            {
                var body = await httpResponse.Content.ReadAsStringAsync();
                var errorResponse = TryDeserialize<NewsApiResponse>(body);
                string apiMsg = errorResponse?.Message ?? "Unknown API error";
                string apiCode = errorResponse?.Code ?? "";

                _logger.LogWarning("NewsAPI error {StatusCode}: {Message}", httpResponse.StatusCode, apiMsg);

                throw httpResponse.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new InvalidApiKeyException(),
                    HttpStatusCode.TooManyRequests => new RateLimitExceededException(),
                    _ => new NewsApiException(
                        $"NewsAPI returned {(int)httpResponse.StatusCode}: {apiMsg}",
                        (int)httpResponse.StatusCode, apiCode)
                };
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            var result = TryDeserialize<T>(json)
                ?? throw new NewsApiException("Failed to deserialize API response.");

            // API can return status=error with HTTP 200
            if (result is NewsApiResponse newsResult && !newsResult.IsSuccess)
            {
                throw newsResult.Code switch
                {
                    "apiKeyInvalid" or "apiKeyMissing" => new InvalidApiKeyException(),
                    "rateLimited" => new RateLimitExceededException(),
                    _ => new NewsApiException(newsResult.Message ?? "NewsAPI error", apiCode: newsResult.Code)
                };
            }

            return result;
        }

        private static T? TryDeserialize<T>(string json)
        {
            try { return JsonConvert.DeserializeObject<T>(json); }
            catch { return default; }
        }
    }
}

