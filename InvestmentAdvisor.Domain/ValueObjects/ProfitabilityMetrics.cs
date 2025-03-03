namespace Domain.ValueObjects
{
    /// <summary>
    /// Profitabilita (ROE, ROA, marže, ROIC, trendy)
    /// </summary>
    public record ProfitabilityMetrics(
        decimal ROE,
        decimal ROA,
        decimal GrossMargin,
        decimal OperatingMargin,
        decimal NetMargin,
        decimal ROIC = 0,
        decimal MarginTrend = 0,
        decimal AssetTurnover = 0);  // Obrat aktiv (tržby/aktiva)
}