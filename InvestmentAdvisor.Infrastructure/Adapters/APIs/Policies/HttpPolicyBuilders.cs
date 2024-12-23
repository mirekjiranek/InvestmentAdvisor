using Polly;
using Polly.Extensions.Http;

namespace Infrastructure.Adapters.APIs.Policies
{
    public static class HttpPolicyBuilders
    {
        public static PolicyBuilder<HttpResponseMessage> GetBaseBuilder()
        {
            return HttpPolicyExtensions.HandleTransientHttpError();
        }
    }
}
