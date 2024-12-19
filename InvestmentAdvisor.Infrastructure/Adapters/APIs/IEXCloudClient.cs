using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.APIs
{
    public class IEXCloudClient
    {
        private readonly HttpClient _httpClient;

        public IEXCloudClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetQuoteAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"stock/{symbol}/quote?token=YOUR_API_KEY");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Další metody pro čtení analytických odhadů, cílových cen atd.
    }
}
