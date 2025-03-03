using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.APIs
{
    public class IEXCloudClient : IIEXCloudClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public IEXCloudClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiKeys:IEXCloud"];
        }

        public async Task<string> GetQuoteAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"stock/{symbol}/quote?token={_apiKey}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Další metody pro čtení analytických odhadů, cílových cen atd.
    }
}