using NewsApp.Models.Api;
using NewsApp.Models.ViewModels;

namespace NewsApp.Services.Interfaces
{
    public interface INewsService
    {
        /// <summary>Fetch top headlines with optional category/country/query filtering.</summary>
        Task<(List<Article> Articles, int TotalResults, bool FromCache)>
            GetTopHeadlinesAsync(TopHeadlinesQuery query);

        /// <summary>
        /// Country-aware headline fetch.
        /// Strategy: try top-headlines with country param first; if fewer than
        /// 3 results come back, fall back to source-based filtering then to
        /// an "everything" search restricted to that country's language.
        /// </summary>
        Task<(List<Article> Articles, int TotalResults, bool FromCache, string Strategy)>
            GetHeadlinesByCountryAsync(string country, string category, int page, int pageSize);


        /// <summary>Full-text search across all articles.</summary>
        Task<(List<Article> Articles, int TotalResults, bool FromCache)>
            SearchEverythingAsync(NewsSearchQuery query);

        /// <summary>Retrieve available news sources with optional filters.</summary>
        Task<(List<SourceDto> Sources, bool FromCache)>
            GetSourcesAsync(string? category = null, string? language = null, string? country = null);

        /// <summary>Fetch a small set of related articles for the detail page.</summary>
        Task<List<Article>> GetRelatedArticlesAsync(string title, int count = 6);
    }
}
