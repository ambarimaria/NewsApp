namespace NewsApp.Infrastructure.Exceptions
{
    /// <summary>Base exception for all NewsApp domain errors.</summary>
    public class NewsAppException : Exception
    {
        public NewsAppException(string message, Exception? inner = null)
            : base(message, inner) { }
    }

    /// <summary>Thrown when the NewsAPI returns a non-success HTTP status.</summary>
    public class NewsApiException : NewsAppException
    {
        public int? StatusCode { get; }
        public string? ApiCode { get; }

        public NewsApiException(string message, int? statusCode = null, string? apiCode = null, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            ApiCode = apiCode;
        }
    }

    /// <summary>Thrown when the API key is missing or invalid.</summary>
    public class InvalidApiKeyException : NewsApiException
    {
        public InvalidApiKeyException()
            : base("NewsAPI key is missing or invalid. Please configure a valid key in appsettings.json.", 401, "apiKeyInvalid") { }
    }

    /// <summary>Thrown when the API rate limit is exceeded.</summary>
    public class RateLimitExceededException : NewsApiException
    {
        public RateLimitExceededException()
            : base("NewsAPI rate limit exceeded. Please wait before making more requests.", 429, "rateLimited") { }
    }
}
