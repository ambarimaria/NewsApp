using NewsApp.Models.Api;

namespace NewsApp.Models.ViewModels
{
    // ── Domain Article (mapped from DTO) ──────────────────────────────────────

    public class Article
    {
        public string? SourceId { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Content { get; set; }

        // Helpers
        public string DisplayAuthor => string.IsNullOrWhiteSpace(Author) ? SourceName : Author;
        public string TimeAgo => PublishedAt.HasValue ? FormatTimeAgo(PublishedAt.Value) : "Unknown time";
        public string ShortDescription => string.IsNullOrWhiteSpace(Description)
            ? "No description available."
            : Description.Length > 160 ? Description[..157] + "…" : Description;
        public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl)
                                && !ImageUrl.Contains("removed.png", StringComparison.OrdinalIgnoreCase);

        private static string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt.ToUniversalTime();
            return diff.TotalMinutes < 1 ? "Just now"
                : diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes}m ago"
                : diff.TotalHours < 24 ? $"{(int)diff.TotalHours}h ago"
                : diff.TotalDays < 7 ? $"{(int)diff.TotalDays}d ago"
                : dt.ToString("MMM d, yyyy");
        }

        /// <summary>Map from API DTO to domain model.</summary>
        public static Article FromDto(ArticleDto dto) => new()
        {
            SourceId = dto.Source?.Id,
            SourceName = dto.Source?.Name ?? "Unknown Source",
            Author = dto.Author,
            Title = dto.Title,
            Description = dto.Description,
            Url = dto.Url,
            ImageUrl = dto.UrlToImage,
            PublishedAt = dto.PublishedAt,
            Content = dto.Content
        };
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public int TotalPages => TotalResults == 0 ? 1
            : (int)Math.Ceiling((double)TotalResults / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int StartIndex => (CurrentPage - 1) * PageSize + 1;
        public int EndIndex => Math.Min(CurrentPage * PageSize, TotalResults);
    }

    // ── View Models ───────────────────────────────────────────────────────────

    public class HeadlinesViewModel
    {
        public List<Article> Articles { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
        public string SelectedCategory { get; set; } = "general";
        public string SelectedCountry { get; set; } = "us";
        public string? ErrorMessage { get; set; }
        public bool IsFromCache { get; set; }
        public string FetchStrategy { get; set; } = "top-headlines"; // "top-headlines" | "sources" | "search"
        public string CountryDisplayName { get; set; } = "United States";
        public List<string> AvailableCategories { get; set; } = NewsConstants.Categories;
        public List<KeyValuePair<string, string>> AvailableCountries { get; set; } = NewsConstants.Countries;
    }

    public class SearchViewModel
    {
        public List<Article> Articles { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
        public string Query { get; set; } = string.Empty;
        public string SortBy { get; set; } = "publishedAt";
        public string Language { get; set; } = "en";
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? SelectedSource { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsFromCache { get; set; }
        public List<SourceItem> Sources { get; set; } = new();
    }

    public class ArticleDetailViewModel
    {
        public Article Article { get; set; } = new();
        public List<Article> RelatedArticles { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class SourcesViewModel
    {
        public List<SourceDto> Sources { get; set; } = new();
        public string? FilterCategory { get; set; }
        public string? FilterLanguage { get; set; }
        public string? FilterCountry { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> AvailableCategories { get; set; } = NewsConstants.Categories;
    }

    public class ErrorViewModel
    {
        public string Title { get; set; } = "An error occurred";
        public string Message { get; set; } = "Something went wrong.";
        public int? StatusCode { get; set; }
        public string? RequestId { get; set; }
    }

    // ── Misc helpers ──────────────────────────────────────────────────────────

    public class SourceItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // ── Query object (service layer) ──────────────────────────────────────────

    public class NewsSearchQuery
    {
        public string Query { get; set; } = string.Empty;
        public string? Sources { get; set; }
        public string? Language { get; set; }
        public string SortBy { get; set; } = "publishedAt";
        public string? From { get; set; }
        public string? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class TopHeadlinesQuery
    {
        public string? Country { get; set; } = "us";
        public string? Category { get; set; } = "general";
        public string? Sources { get; set; }
        public string? Query { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}

