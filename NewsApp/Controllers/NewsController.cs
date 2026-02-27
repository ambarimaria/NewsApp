using Microsoft.AspNetCore.Mvc;
using NewsApp.Models;
using NewsApp.Models.ViewModels;
using NewsApp.Services.Interfaces;

namespace NewsApp.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsService newsService, ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        // ── Headlines ─────────────────────────────────────────────────────────

        /// <summary>GET /News  — top headlines with 3-tier country filtering.</summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string category = "general",
            string country = "us",
            int page = 1)
        {
            int pageSize = 12;
            page = Math.Max(1, page);

            // Resolve display name for the selected country
            var countryDisplay = NewsConstants.Countries
                .FirstOrDefault(kv => kv.Key == country).Value ?? country.ToUpper();

            var vm = new HeadlinesViewModel
            {
                SelectedCategory = category,
                SelectedCountry = country,
                CountryDisplayName = countryDisplay,
                Pagination = new() { CurrentPage = page, PageSize = pageSize }
            };

            try
            {
                // Uses 3-tier fallback: top-headlines → sources → everything search
                var (articles, total, fromCache, strategy) =
                    await _newsService.GetHeadlinesByCountryAsync(country, category, page, pageSize);

                vm.Articles = articles;
                vm.Pagination.TotalResults = total;
                vm.IsFromCache = fromCache;
                vm.FetchStrategy = strategy;

                _logger.LogInformation(
                    "Headlines loaded: country={C} category={Cat} strategy={S} count={N}",
                    country, category, strategy, articles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading headlines for country={C}", country);
                vm.ErrorMessage = "Could not load headlines right now. Please try again shortly.";
            }

            return View(vm);
        }

        // ── Search ────────────────────────────────────────────────────────────

        /// <summary>GET /News/Search?q=…&sortBy=…&language=…&page=…</summary>
        [HttpGet]
        public async Task<IActionResult> Search(
            string q = "",
            string sortBy = "publishedAt",
            string language = "en",
            string? source = null,
            string? from = null,
            string? to = null,
            int page = 1)
        {
            int pageSize = 12;
            page = Math.Max(1, page);

            // Populate source dropdown
            var (sources, _) = await _newsService.GetSourcesAsync(language: language);

            var vm = new SearchViewModel
            {
                Query = q,
                SortBy = sortBy,
                Language = language,
                SelectedSource = source,
                FromDate = from,
                ToDate = to,
                Pagination = new() { CurrentPage = page, PageSize = pageSize },
                Sources = sources.Select(s => new SourceItem { Id = s.Id, Name = s.Name }).ToList()
            };

            if (string.IsNullOrWhiteSpace(q))
                return View(vm);

            try
            {
                var (articles, total, fromCache) = await _newsService.SearchEverythingAsync(
                    new NewsSearchQuery
                    {
                        Query = q,
                        Sources = source,
                        Language = language,
                        SortBy = sortBy,
                        From = from,
                        To = to,
                        Page = page,
                        PageSize = pageSize
                    });

                vm.Articles = articles;
                vm.Pagination.TotalResults = total;
                vm.IsFromCache = fromCache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching: {Query}", q);
                vm.ErrorMessage = "Search failed. Please try different keywords.";
            }

            return View(vm);
        }

        // ── Article Detail ────────────────────────────────────────────────────

        /// <summary>GET /News/Detail?url=…&title=…  — full article detail + related.</summary>
        [HttpGet]
        public async Task<IActionResult> Detail(
            string url,
            string title,
            string? description,
            string? image,
            string? author,
            string? source,
            string? publishedAt)
        {
            if (string.IsNullOrWhiteSpace(url))
                return RedirectToAction(nameof(Index));

            var article = new Article
            {
                Url = url,
                Title = title,
                Description = description,
                ImageUrl = image,
                Author = author,
                SourceName = source ?? "Unknown",
                PublishedAt = DateTime.TryParse(publishedAt, out var dt) ? dt : null
            };

            var vm = new ArticleDetailViewModel { Article = article };

            try
            {
                vm.RelatedArticles = await _newsService.GetRelatedArticlesAsync(title);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load related articles");
            }

            return View(vm);
        }

        // ── Sources ───────────────────────────────────────────────────────────

        /// <summary>GET /News/Sources?category=…&language=…&country=…</summary>
        [HttpGet]
        public async Task<IActionResult> Sources(
            string? category = null,
            string? language = null,
            string? country = null)
        {
            var vm = new SourcesViewModel
            {
                FilterCategory = category,
                FilterLanguage = language,
                FilterCountry = country
            };

            try
            {
                var (sources, _) = await _newsService.GetSourcesAsync(category, language, country);
                vm.Sources = sources;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sources");
                vm.ErrorMessage = "Could not load sources right now.";
            }

            return View(vm);
        }
    }
}

