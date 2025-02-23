using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly;
using Serilog;

namespace BusinessLogicLayer.Policies
{
    public class PollyPolicies :IPollyPolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .WaitAndRetryAsync(
             retryCount: retryCount, //Number of retries
             sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Delay between retries
             onRetry: (outcome, timespan, retryAttempt, context) =>
             {
                 Log.Information($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
             });

            return policy;
        }


        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .CircuitBreakerAsync(
             handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, //Threshold for failed requests
             durationOfBreak: durationOfBreak, // Waiting time to be in "Open" state
             onBreak: (outcome, timespan) =>
             {
                 Log.Information($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
             },
             onReset: () => {
                 Log.Information($"Circuit breaker closed. The subsequent requests will be allowed.");
             });

            return policy;
        }


        public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
        {
            AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

            return policy;
        }
    }
}
