# 📰 NewsApp

> A production-ready **ASP.NET Core 8 MVC** application that delivers real-time news headlines, full-text article search, and source browsing — powered by the [NewsAPI.org](https://newsapi.org) REST API with a polished dark UI.

---

## 📋 Table of Contents

- [Features](#-features)
- [Screenshots Overview](#-screenshots-overview)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Configuration Reference](#-configuration-reference)
- [Architecture Deep Dive](#-architecture-deep-dive)
- [Country Filtering — How It Works](#-country-filtering--how-it-works)
- [Caching Strategy](#-caching-strategy)
- [Resilience & Error Handling](#-resilience--error-handling)
- [API Reference](#-api-reference)
- [NuGet Packages](#-nuget-packages)
- [Known Limitations](#-known-limitations)
- [Extending the Project](#-extending-the-project)

---

## ✨ Features

| Feature | Details |
|---|---|
| 🌍 **Country filtering** | 3-tier fallback strategy guarantees real country-specific news |
| 🗂 **Category browsing** | 7 categories: General, Business, Entertainment, Health, Science, Sports, Technology |
| 🔍 **Advanced search** | Sort by relevancy/date/popularity, filter by language, date range & source |
| 📄 **Article detail** | Full description, hero image, share buttons (X, LinkedIn, copy link) |
| 🔗 **Related articles** | Keyword-extracted related news shown in sidebar |
| 📡 **Sources browser** | Browse all NewsAPI sources, filter by category/language/country |
| ⚡ **In-memory caching** | Deterministic cache keys, configurable TTL, "Cached" badge in UI |
| 🔄 **Polly resilience** | Exponential back-off retry + circuit breaker on all HTTP calls |
| 🛡 **Global error handling** | MVC filter maps API errors to friendly user-facing messages |
| 📱 **Responsive UI** | Mobile-first dark theme, works from 320 px upwards |
| 📖 **Reading progress bar** | Animated gradient bar on article detail pages |

---

## 🖥 Screenshots Overview

```
┌─────────────────────────────────────────────────────────┐
│  📰 NewsApp          [Headlines] [Search] [Sources]  🔍  │
│─────────────────────────────────────────────────────────│
│  General | Business | Entertainment | Health | ...       │
│─────────────────────────────────────────────────────────│
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │           FEATURED HERO ARTICLE                 │   │
│  │         (full-width with overlay)               │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐                  │
│  │Card 1│ │Card 2│ │Card 3│ │Card 4│  ...              │
│  └──────┘ └──────┘ └──────┘ └──────┘                  │
│                                                         │
│           [ ‹ 1  2  3  4  5 › ]                        │
└─────────────────────────────────────────────────────────┘
```

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core 8 MVC |
| **Language** | C# 12 |
| **HTTP Resilience** | Polly v7 (retry + circuit breaker) |
| **JSON** | Newtonsoft.Json 13 |
| **Caching** | Microsoft.Extensions.Caching.Memory |
| **UI** | Bootstrap 5.3, Font Awesome 6, Google Fonts |
| **External API** | [NewsAPI.org](https://newsapi.org) REST API |

---

## 📁 Project Structure

```
NewsApp/
│
├── Controllers/
│   ├── NewsController.cs          # Main controller — Headlines, Search, Detail, Sources
│   └── HomeController.cs          # Redirects root → /News
│
├── Models/
│   ├── Api/
│   │   └── NewsApiDtos.cs         # Raw API response contracts (never leak to Views)
│   ├── ViewModels/
│   │   └── ViewModels.cs          # Domain Article model + all ViewModels + Query objects
│   └── NewsConstants.cs           # Categories, countries, sources map, language map
│
├── Services/
│   ├── Interfaces/
│   │   └── INewsService.cs        # Service contract (5 methods)
│   └── NewsService.cs             # Full implementation with 3-tier country fallback
│
├── Infrastructure/
│   ├── NewsApiSettings.cs         # Strongly-typed config bound from appsettings.json
│   ├── Cache/
│   │   └── CacheKeyBuilder.cs     # Deterministic cache key generation
│   ├── Http/
│   │   └── ResiliencePolicies.cs  # Polly retry + circuit-breaker factory
│   └── Exceptions/
│       └── NewsAppExceptions.cs   # Typed domain exceptions
│
├── Filters/
│   └── GlobalExceptionFilter.cs   # MVC exception filter → friendly error views
│
├── Views/
│   ├── News/
│   │   ├── Index.cshtml           # Headlines page with featured hero + card grid
│   │   ├── Search.cshtml          # Advanced search with collapsible filters
│   │   ├── Detail.cshtml          # Article detail + related sidebar + share
│   │   └── Sources.cshtml         # Sources browser with filter bar
│   └── Shared/
│       ├── _Layout.cshtml         # Dark master layout + navbar + category ribbon
│       ├── _ArticleCard.cshtml    # Reusable article card partial
│       ├── _Pagination.cshtml     # Reusable pagination partial
│       └── Error.cshtml           # User-friendly error page
│
├── wwwroot/
│   ├── css/site.css               # Full dark theme with CSS design tokens
│   └── js/site.js                 # Reading progress bar, scroll effects, validation
│
├── Properties/
│   └── launchSettings.json        # Dev server on http://localhost:5050
│
├── appsettings.json               # Main configuration
├── appsettings.Development.json   # Dev overrides (shorter cache TTL)
└── Program.cs                     # DI wiring, HttpClient, Polly, MVC filter
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) or later
- A free NewsAPI key → [newsapi.org/register](https://newsapi.org/register)

### Step 1 — Clone / Extract the project

```bash
cd NewsApp
```

### Step 2 — Add your API key

Open `appsettings.json` and replace the placeholder:

```json
"NewsApi": {
  "ApiKey": "YOUR_NEWSAPI_KEY_HERE"
}
```

> **Recommended for production:** Use .NET User Secrets instead of storing the key in appsettings.json:
> ```bash
> dotnet user-secrets init
> dotnet user-secrets set "NewsApi:ApiKey" "your_actual_key_here"
> ```

### Step 3 — Restore & Run

```bash
dotnet restore
dotnet run
```

Open your browser at **http://localhost:5050**

---

## ⚙️ Configuration Reference

All settings live under the `"NewsApi"` section in `appsettings.json`:

```json
{
  "NewsApi": {
    "BaseUrl":              "https://newsapi.org/v2/",
    "ApiKey":               "YOUR_KEY_HERE",
    "CacheDurationMinutes": 5,
    "DefaultPageSize":      12,
    "MaxPageSize":          100,
    "TimeoutSeconds":       15,
    "RetryCount":           3,
    "RetryDelaySeconds":    2
  },
  "App": {
    "DefaultCategory": "general",
    "DefaultCountry":  "us",
    "DefaultLanguage": "en"
  }
}
```

| Setting | Default | Description |
|---|---|---|
| `ApiKey` | — | **Required.** Your NewsAPI key |
| `CacheDurationMinutes` | `5` | How long API responses are cached. Sources are cached 6× longer |
| `DefaultPageSize` | `12` | Articles per page |
| `TimeoutSeconds` | `15` | HTTP request timeout |
| `RetryCount` | `3` | Polly retry attempts on transient failures |
| `RetryDelaySeconds` | `2` | Base delay for exponential back-off (2s, 4s, 8s) |

---

## 🏗 Architecture Deep Dive

### Dependency Injection (Program.cs)

```csharp
// Strongly-typed config — injected as singletons
builder.Services.AddSingleton(apiSettings);

// Named HttpClient with Polly pipeline
builder.Services
    .AddHttpClient("NewsApi", client => { ... })
    .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy())
    .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy());

// Scoped service
builder.Services.AddScoped<INewsService, NewsService>();

// Global exception filter registered on MVC
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add<GlobalExceptionFilter>();
});
```

### Service Contract (INewsService)

```csharp
public interface INewsService
{
    Task<(List<Article>, int, bool)>          GetTopHeadlinesAsync(TopHeadlinesQuery q);
    Task<(List<Article>, int, bool, string)>  GetHeadlinesByCountryAsync(string country, string category, int page, int pageSize);
    Task<(List<Article>, int, bool)>          SearchEverythingAsync(NewsSearchQuery q);
    Task<(List<SourceDto>, bool)>             GetSourcesAsync(string? category, string? language, string? country);
    Task<List<Article>>                       GetRelatedArticlesAsync(string title, int count);
}
```

### Data Flow

```
Browser Request
      │
      ▼
NewsController.Index(country, category, page)
      │
      ▼
NewsService.GetHeadlinesByCountryAsync()
      │
      ├─ Check IMemoryCache ──► HIT: return cached result
      │
      └─ MISS: FetchAsync() via named HttpClient
                    │
                    ├─ Polly Retry (exponential back-off)
                    ├─ Polly Circuit Breaker
                    │
                    ▼
              NewsAPI REST endpoint
                    │
                    ▼
         Deserialize → FilterArticles → Cache → Return
      │
      ▼
HeadlinesViewModel → Index.cshtml → HTML Response
```

---

## 🌍 Country Filtering — How It Works

NewsAPI's free plan silently ignores the `country` parameter for many countries. To guarantee real country-specific news, `GetHeadlinesByCountryAsync` uses a **3-tier fallback strategy**:

```
┌─────────────────────────────────────────────────────────────┐
│  Strategy 1: top-headlines?country={code}&category={cat}    │
│  ✅ Works for: US, GB, AU, CA, IN, DE, FR, IT, JP, ...      │
│  ❌ Falls through if: fewer than 3 results returned          │
└────────────────────────┬────────────────────────────────────┘
                         │ < 3 results
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  Strategy 2: top-headlines?sources={country_source_ids}     │
│  Uses hardcoded list of known sources per country           │
│  e.g. GB → bbc-news, the-guardian-uk, sky-news, ...        │
│  ✅ Works reliably on free tier                             │
│  ❌ Falls through if: no sources mapped or 0 results        │
└────────────────────────┬────────────────────────────────────┘
                         │ 0 results
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  Strategy 3: everything?q={category} {CountryName}          │
│                         &language={country_language}         │
│  Broadest fallback — full-text search in country language   │
│  e.g. DE → q="technology Germany"&language=de               │
└─────────────────────────────────────────────────────────────┘
```

The UI shows a subtle badge indicating which strategy was used:
- *(no badge)* — native `top-headlines` worked perfectly
- **`via Sources`** — Strategy 2 was used
- **`via Search Fallback`** — Strategy 3 was used

**Country → Source mappings** are defined in `NewsConstants.CountrySources` and cover 15 countries including US, GB, AU, CA, IN, DE, FR, IT, JP, BR, MX, ZA, AE, SG, NZ.

---

## ⚡ Caching Strategy

Every unique API call is cached with a **deterministic key** built by `CacheKeyBuilder`:

```
NewsApp:top:c=gb|cat=technology|src=|q=|p=1|ps=12
NewsApp:all:q=bitcoin|src=|lang=en|sort=publishedAt|from=|to=|p=1|ps=12
NewsApp:sources:cat=|lang=en|c=
country_s1_gb_technology_1_12
country_s2_gb_technology_1_12
country_s3_gb_technology_1_12
```

| Cache type | TTL |
|---|---|
| Headlines & search results | `CacheDurationMinutes` (default 5 min) |
| News sources list | `CacheDurationMinutes × 6` (default 30 min) |

A green **⚡ Cached** badge is shown in the UI when a response is served from cache.

---

## 🛡 Resilience & Error Handling

### Polly Pipeline

Two policies are chained on the `"NewsApi"` HttpClient:

```
Request → [Circuit Breaker] → [Retry] → NewsAPI
```

| Policy | Config | Behaviour |
|---|---|---|
| **Retry** | 3 attempts, base 2s delay | Retries on 5xx, network errors, timeouts and 429. Waits 2s → 4s → 8s |
| **Circuit Breaker** | 5 failures, 30s open | Opens after 5 consecutive failures; half-opens after 30s to test recovery |

### Exception Hierarchy

```
NewsAppException
└── NewsApiException
    ├── InvalidApiKeyException    → HTTP 401 → "Add your API key" message
    └── RateLimitExceededException → HTTP 429 → "Wait and retry" message
```

### GlobalExceptionFilter

Registered as an MVC filter — catches **all** unhandled exceptions and maps them to a friendly `Error.cshtml` view with the correct HTTP status code. In Development mode, full stack traces are shown.

---

## 🔌 API Reference

The app consumes three NewsAPI v2 endpoints:

| Endpoint | Used for |
|---|---|
| `GET /v2/top-headlines` | Category headlines, country headlines, source-based headlines |
| `GET /v2/everything` | Full-text search, related articles, Strategy 3 country fallback |
| `GET /v2/top-headlines/sources` | Sources browser, populating search source dropdown |

**Authentication:** API key is sent as the `X-Api-Key` request header (more secure than a query param).

**Free tier limits:**
- 100 requests / day in developer mode
- `top-headlines` supports: `country`, `category`, `sources`, `q`, `page`, `pageSize`
- `everything` supports: `q`, `sources`, `language`, `sortBy`, `from`, `to`, `page`, `pageSize`
- Cannot mix `sources` with `country` or `category` in the same request

---

## 📦 NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Newtonsoft.Json` | 13.0.3 | JSON deserialization of API responses |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | 8.0.0 | MVC Newtonsoft integration |
| `Microsoft.Extensions.Http.Polly` | **7.0.20** | `AddPolicyHandler` extension on IHttpClientBuilder |
| `Polly` | **7.2.4** | Retry & circuit-breaker policies |
| `Polly.Extensions.Http` | 3.0.0 | `HttpPolicyExtensions.HandleTransientHttpError()` |
| `Microsoft.Extensions.Caching.Memory` | 8.0.0 | In-memory response cache |

> ⚠️ **Important:** `Microsoft.Extensions.Http.Polly` must be **v7**, not v8.
> Version 8 removed `AddPolicyHandler` and introduced a new incompatible API.
> Both v7 packages work fully on .NET 8 at runtime.

---

## ⚠️ Known Limitations

| Limitation | Reason | Workaround |
|---|---|---|
| Free plan: 100 req/day | NewsAPI developer tier | Increase `CacheDurationMinutes` to reduce calls |
| Free plan: no HTTPS in production | NewsAPI blocks free keys on non-localhost | Upgrade to paid plan for deployment |
| Country filter may use fallback | NewsAPI silently ignores unknown country codes | The 3-tier strategy handles this automatically |
| `sources` + `category` conflict | NewsAPI restriction | Strategy 2 omits category when using sources |
| Article content truncated at 200 chars | NewsAPI free tier limit | Full content requires paid plan or scraping |

---

## 🔧 Extending the Project

### Swap the API
Implement `INewsService` against any other news provider (Google News, NY Times, Guardian, etc.) without changing controllers or views.

### Add a Database
Use EF Core + SQL Server to persist user favourites or reading history:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Add Authentication
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

### Deploy to Azure
```bash
dotnet publish -c Release
az webapp create --name newsapp --runtime "DOTNET|8.0"
az webapp config appsettings set --settings NewsApi__ApiKey="your_key"
```

### Upgrade to Paid NewsAPI Plan
Update `appsettings.json` — no code changes needed. Paid plan removes the 100 req/day limit and enables production HTTPS deployment.

---

## 📄 License

This project is provided for educational purposes. News content is sourced from [NewsAPI.org](https://newsapi.org) — please review their [terms of service](https://newsapi.org/terms) before deploying publicly.

---

<p align="center">
  Built with ❤️ using <strong>ASP.NET Core 8 MVC</strong> &nbsp;·&nbsp; Data by <a href="https://newsapi.org">NewsAPI.org</a>
</p>

