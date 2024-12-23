namespace Infrastructure.Adapters.APIs
{
    public class FinnhubClient : IFinnhubClient
    {
        private readonly HttpClient _httpClient;

        public FinnhubClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetCompanyProfileAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"stock/profile2?symbol={symbol}&token=YOUR_API_KEY");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
