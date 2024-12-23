namespace Domain.ValueObjects
{
    /// <summary>
    /// Value Object obsahující metriky dividend.
    /// </summary>
    public record DividendMetrics(
        decimal DividendYield,         // Výnos z dividendy (např. 3 %)
        decimal DividendPayoutRatio,   // Poměr výplaty dividendy
        decimal DividendGrowth,        // Růst dividendy
        decimal CurrentAnnualDividend  // Aktuální roční dividenda
    );
}
