namespace Domain.ValueObjects
{
    /// <summary>
    /// Finanční stabilita (Debt/Equity, Current ratio)
    /// </summary>
    public record StabilityMetrics(
        decimal DebtToEquity,
        decimal CurrentRatio,
        decimal QuickRatio,
        decimal InterestCoverage);
}
