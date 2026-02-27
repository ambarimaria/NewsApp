using System.Text;

namespace NewsApp.Infrastructure.Cache
{
    /// <summary>
    /// Deterministically builds memory-cache keys so every unique query
    /// gets its own slot while identical queries share one.
    /// </summary>
    public static class CacheKeyBuilder
    {
        private const string Prefix = "NewsApp";

        public static string TopHeadlines(
            string? country, string? category, string? sources, string? query, int page, int pageSize)
        {
            var sb = new StringBuilder($"{Prefix}:top:");
            sb.Append($"c={country?.ToLower() ?? ""}|");
            sb.Append($"cat={category?.ToLower() ?? ""}|");
            sb.Append($"src={sources?.ToLower() ?? ""}|");
            sb.Append($"q={query?.ToLower() ?? ""}|");
            sb.Append($"p={page}|ps={pageSize}");
            return sb.ToString();
        }

        public static string Everything(
            string query, string? sources, string? language,
            string? sortBy, string? from, string? to, int page, int pageSize)
        {
            var sb = new StringBuilder($"{Prefix}:all:");
            sb.Append($"q={query.ToLower()}|");
            sb.Append($"src={sources?.ToLower() ?? ""}|");
            sb.Append($"lang={language?.ToLower() ?? ""}|");
            sb.Append($"sort={sortBy?.ToLower() ?? ""}|");
            sb.Append($"from={from ?? ""}|to={to ?? ""}|");
            sb.Append($"p={page}|ps={pageSize}");
            return sb.ToString();
        }

        public static string Sources(string? category, string? language, string? country)
            => $"{Prefix}:sources:cat={category ?? ""}|lang={language ?? ""}|c={country ?? ""}";
    }
}
