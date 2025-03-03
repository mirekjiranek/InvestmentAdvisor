namespace Domain.ValueObjects
{
    /// <summary>
    /// Rizikové ukazatele (Beta, Sharpe ratio, volatilita)
    /// </summary>
    public record MarketRiskMetrics(
        decimal Beta,
        decimal SharpeRatio,
        decimal StandardDeviation)
    {
        // Alias pro StandardDeviation
        public decimal Volatility => StandardDeviation;
    }
}