using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NewsApp.Infrastructure.Exceptions;
using NewsApp.Models.ViewModels;

namespace NewsApp.Filters
{
    /// <summary>
    /// MVC exception filter that catches all unhandled exceptions,
    /// logs them, and redirects to a friendly error view instead of
    /// crashing with a 500 stack trace.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionFilter(
            ILogger<GlobalExceptionFilter> logger,
            ITempDataDictionaryFactory tempDataFactory,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _tempDataFactory = tempDataFactory;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            string title;
            string message;
            int statusCode = 500;

            switch (ex)
            {
                case InvalidApiKeyException:
                    title = "Invalid API Key";
                    message = "Your NewsAPI key is missing or invalid. Please add a valid API key to appsettings.json under \"NewsApi:ApiKey\". You can get a free key at https://newsapi.org/register";
                    statusCode = 401;
                    _logger.LogWarning("Invalid NewsAPI key");
                    break;

                case RateLimitExceededException:
                    title = "Rate Limit Exceeded";
                    message = "You've made too many requests to NewsAPI. Please wait a moment and try again. Free plans allow 100 requests/day.";
                    statusCode = 429;
                    _logger.LogWarning("NewsAPI rate limit hit");
                    break;

                case NewsApiException apiEx:
                    title = "News API Error";
                    message = apiEx.Message;
                    statusCode = apiEx.StatusCode ?? 500;
                    _logger.LogError(ex, "NewsAPI exception: {Message}", apiEx.Message);
                    break;

                default:
                    title = "Unexpected Error";
                    message = _env.IsDevelopment()
                        ? ex.ToString()
                        : "An unexpected error occurred. Please try again later.";
                    _logger.LogError(ex, "Unhandled exception");
                    break;
            }

            var errorVm = new ErrorViewModel
            {
                Title = title,
                Message = message,
                StatusCode = statusCode,
                RequestId = System.Diagnostics.Activity.Current?.Id
                            ?? context.HttpContext.TraceIdentifier
            };

            context.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary<ErrorViewModel>(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                    new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                {
                    Model = errorVm
                }
            };

            context.HttpContext.Response.StatusCode = statusCode;
            context.ExceptionHandled = true;
        }
    }
}
