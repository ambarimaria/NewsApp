using NewsApp.Filters;
using NewsApp.Infrastructure;
using NewsApp.Infrastructure.Http;
using NewsApp.Services;
using NewsApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Strongly-typed config ─────────────────────────────────────────────────────
var apiSettings = builder.Configuration
    .GetSection(NewsApiSettings.SectionName)
    .Get<NewsApiSettings>() ?? new NewsApiSettings();

var appSettings = builder.Configuration
    .GetSection(AppSettings.SectionName)
    .Get<AppSettings>() ?? new AppSettings();

// Register as singletons so they can be injected directly
builder.Services.AddSingleton(apiSettings);
builder.Services.AddSingleton(appSettings);

// ── HTTP Client + Polly resilience ────────────────────────────────────────────
builder.Services
    .AddHttpClient("NewsApi", client =>
    {
        client.BaseAddress = new Uri(apiSettings.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "NewsApp/1.0");
        // Attach API key as header (recommended over query param)
        client.DefaultRequestHeaders.Add("X-Api-Key", apiSettings.ApiKey);
    })
    .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy(
        apiSettings.RetryCount, apiSettings.RetryDelaySeconds))
    .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy());

// ── App services ──────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddScoped<INewsService, NewsService>();

// ── MVC with global exception filter ─────────────────────────────────────────
builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddNewtonsoftJson();

// ── Response caching for static assets ───────────────────────────────────────
builder.Services.AddResponseCaching();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseResponseCaching();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=News}/{action=Index}/{id?}");

app.Run();

