namespace Domain.ValueObjects
{
    /// <summary>
    /// Profitabilita (ROE, ROA, marže)
    /// </summary>
    public record ProfitabilityMetrics(
        decimal ROE,
        decimal ROA,
        decimal GrossMargin,
        decimal OperatingMargin,
        decimal NetMargin);
}
