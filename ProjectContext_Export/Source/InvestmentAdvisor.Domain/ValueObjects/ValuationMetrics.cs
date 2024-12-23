namespace Domain.ValueObjects
{
    /// <summary>
    /// Valuační metriky jako PE, PB, EV/EBITDA...
    /// Value object - porovnává se podle hodnot, ne identity.
    /// </summary>
    public record ValuationMetrics(
        decimal PE,
        decimal PB,
        decimal EV_EBITDA,
        decimal EV_EBIT,
        decimal PriceSales,
        decimal PriceCashFlow);
}

