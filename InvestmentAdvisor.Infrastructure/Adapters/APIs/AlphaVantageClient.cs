using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.APIs
{
    public class AlphaVantageClient : IAlphaVantageClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AlphaVantageClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiKeys:AlphaVantage"];
        }

        public async Task<string> GetDailyPricesAsync(string symbol, string outputSize = "compact")
        {
            var uri = $"query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize={outputSize}&apikey={_apiKey}";
            return await GetJsonAsync(uri);
        }

        public async Task<string> GetCompanyOverviewAsync(string symbol)
        {
            var uri = $"query?function=OVERVIEW&symbol={symbol}&apikey={_apiKey}";
            return await GetJsonAsync(uri);
        }

        public async Task<string> GetEarningsAsync(string symbol)
        {
            var uri = $"query?function=EARNINGS&symbol={symbol}&apikey={_apiKey}";
            return await GetJsonAsync(uri);
        }

        public async Task<string> GetIncomeStatementAsync(string symbol)
        {
            var uri = $"query?function=INCOME_STATEMENT&symbol={symbol}&apikey={_apiKey}";
            return await GetJsonAsync(uri);
        }

        public async Task<string> GetBalanceSheetAsync(string symbol)
        {
            var uri = $"query?function=BALANCE_SHEET&symbol={symbol}&apikey={_apiKey}";
            return await GetJsonAsync(uri);
        }

        public async Task<string> GetCashFlowAsync(string symbol)
        {
            var uri = $"query?function=CASH_FLOW&symbol={symbol}&apikey={_apiKey}";
            return await GetJsonAsync(uri);
        }

        private async Task<string> GetJsonAsync(string relativeUri)
        {
            var response = await _httpClient.GetAsync(relativeUri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}