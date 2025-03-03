namespace Infrastructure.Adapters.APIs
{
    public interface IAlphaVantageClient
    {
        Task<string> GetDailyPricesAsync(string symbol, string outputSize = "compact");
        Task<string> GetCompanyOverviewAsync(string symbol);
        Task<string> GetEarningsAsync(string symbol);
        Task<string> GetIncomeStatementAsync(string symbol);
        Task<string> GetBalanceSheetAsync(string symbol);
        Task<string> GetCashFlowAsync(string symbol);
    }
}
