namespace Domain.ValueObjects
{
    /// <summary>
    /// Finanční stabilita (Debt/Equity, Current ratio)
    /// </summary>
    public record StabilityMetrics(
        decimal DebtToEquity,
        decimal CurrentRatio,
        decimal QuickRatio,
        decimal InterestCoverage,
        decimal EarningsStability = 0, // Stabilita zisků (0-1, kde 1 = velmi stabilní)
        bool IsDataVerified = false);
}