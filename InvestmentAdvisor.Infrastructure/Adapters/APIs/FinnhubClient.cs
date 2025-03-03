using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.APIs
{
    public class FinnhubClient : IFinnhubClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FinnhubClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiKeys:Finnhub"];
        }

        public async Task<string> GetCompanyProfileAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"stock/profile2?symbol={symbol}&token={_apiKey}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}