using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Adapters.APIs.Policies
{
    public static class HttpRetryPolicies
    {
        public static RetryPolicy<HttpResponseMessage> GetHttpRetryPolicy(ILogger logger, IRetryPolicyConfig retryPolicyConfig)
        {
            return HttpPolicyBuilders.GetBaseBuilder()
                                      .WaitAndRetry(
                                          retryCount: retryPolicyConfig.RetryCount,
                                          sleepDurationProvider: retryAttempt => ComputeDuration(retryAttempt),
                                          onRetry: (outcome, timespan, retryAttempt, context) =>
                                          {
                                              OnHttpRetry(outcome, timespan, retryAttempt, context, logger);
                                          });
        }

        private static void OnHttpRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Polly.Context context, ILogger logger)
        {
            if (result.Result != null)
            {
                logger.LogWarning("Request failed with {StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}", result.Result.StatusCode, timeSpan, retryCount);
            }
            else
            {
                logger.LogWarning("Request failed due to network failure. Waiting {timeSpan} before next retry. Retry attempt {retryCount}", timeSpan, retryCount);
            }
        }

        private static TimeSpan ComputeDuration(int retryAttempt)
        {
            // Exponential backoff with jitter
            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(new Random().Next(0, 100));
        }
    }

}
