using Polly.CircuitBreaker;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Adapters.APIs.Policies
{
    public class HttpCircuitBreakerPolicies
    {
        public static CircuitBreakerPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy(ILogger logger, ICircuitBreakerPolicyConfig circuitBreakerPolicyConfig)
        {
            return HttpPolicyBuilders.GetBaseBuilder()
                                      .CircuitBreaker(
                                          handledEventsAllowedBeforeBreaking: circuitBreakerPolicyConfig.RetryCount + 1,
                                          durationOfBreak: TimeSpan.FromSeconds(circuitBreakerPolicyConfig.BreakDuration),
                                          onBreak: (outcome, breakDuration) =>
                                          {
                                              OnHttpBreak(outcome, breakDuration, circuitBreakerPolicyConfig.RetryCount, logger);
                                          },
                                          onReset: () =>
                                          {
                                              OnHttpReset(logger);
                                          });
        }

        private static void OnHttpBreak(DelegateResult<HttpResponseMessage> outcome, TimeSpan breakDuration, int retryCount, ILogger logger)
        {
            logger.LogWarning("Circuit breaker opened for {breakDuration} seconds after {retryCount} failed retries.", breakDuration.TotalSeconds, retryCount);
            // Pokud nechcete vyhazovat výjimku, můžete tuto část vynechat.
            // throw new BrokenCircuitException("Service inoperative. Please try again later");
        }

        private static void OnHttpReset(ILogger logger)
        {
            logger.LogInformation("Circuit breaker reset.");
        }
    }
}
