using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Serilog;
using System;

namespace BusinessLogicLayer.Policies
{
    //injected this as a Transient service in Program.cs
    public class UsersMicroservicePolicies : IUsersMicroservicePolicies
    {
        private readonly IPollyPolicies _pollyPolicies;

        public UsersMicroservicePolicies(IPollyPolicies pollyPolicies)
        {
            _pollyPolicies = pollyPolicies;
        }

        //combining all the policies to dont get any problems in the order wise execution
        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
            var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
            var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));
            AsyncPolicyWrap<HttpResponseMessage> policy =  Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
            return policy;
        }
    }
}
