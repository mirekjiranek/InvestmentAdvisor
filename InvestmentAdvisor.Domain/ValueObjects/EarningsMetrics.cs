namespace Domain.ValueObjects
{
    /// <summary>
    /// Value Object obsahující metriky výnosů společnosti.
    /// </summary>
    public record EarningsMetrics(
        decimal EPS,    // Zisk na akcii (Earnings Per Share)
        decimal EBITDA  // Zisk před úroky, daněmi a odpisy
    );
}
