using Polly;
using Polly.Extensions.Http;

namespace NewsApp.Infrastructure.Http
{
    /// <summary>
    /// Provides Polly retry and circuit-breaker policies for outbound HTTP calls.
    /// </summary>
    public static class ResiliencePolicies
    {
        /// <summary>
        /// Exponential back-off retry: retries <paramref name="retryCount"/> times
        /// on transient HTTP errors or timeouts.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3, int delaySeconds = 2)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => (int)r.StatusCode == 429) // also retry on rate-limit
                .WaitAndRetryAsync(
                    retryCount,
                    attempt => TimeSpan.FromSeconds(Math.Pow(delaySeconds, attempt)),
                    onRetry: (outcome, timeSpan, attempt, _) =>
                    {
                        // Logging is done via ILogger injected separately;
                        // here we just record to debug output for visibility.
                        System.Diagnostics.Debug.WriteLine(
                            $"[Polly] Retry {attempt} after {timeSpan.TotalSeconds:0.0}s " +
                            $"due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });
        }

        /// <summary>
        /// Circuit-breaker: opens after 5 consecutive failures, stays open for 30 s.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (_, duration) =>
                        System.Diagnostics.Debug.WriteLine($"[Polly] Circuit OPEN for {duration.TotalSeconds}s"),
                    onReset: () =>
                        System.Diagnostics.Debug.WriteLine("[Polly] Circuit CLOSED"));
        }
    }
}
