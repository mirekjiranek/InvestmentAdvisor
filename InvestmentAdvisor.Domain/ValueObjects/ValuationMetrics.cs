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
        decimal PriceCashFlow,
        decimal ForwardEPS = 0,
        decimal BookValuePerShare = 0,
        decimal PEG = 0)
    {
        // Aliasy pro různé názvy stejných properties
        public decimal PriceBook => PB;
        public decimal EvEbitda => EV_EBITDA;
        public decimal EVEBITDA => EV_EBITDA;
    }
}