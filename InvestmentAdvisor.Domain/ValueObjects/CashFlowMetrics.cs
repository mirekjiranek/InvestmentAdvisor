namespace Domain.ValueObjects
{
    /// <summary>
    /// Value Object obsahující metriky volného cash flow (FCF).
    /// </summary>
    public record CashFlowMetrics(
        decimal CurrentFCF,        // Aktuální roční Free Cash Flow
        decimal ProjectedFCFGrowth // Předpokládaný růst FCF v procentech (např. 0.08 = 8 % ročně)
    );
}
