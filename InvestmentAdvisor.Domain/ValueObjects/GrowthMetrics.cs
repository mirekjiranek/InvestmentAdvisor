namespace Domain.ValueObjects
{
    /// <summary>
    /// Růstové ukazatele.
    /// </summary>
    public record GrowthMetrics(
        decimal HistoricalEpsGrowth,    // Historický růst EPS
        decimal PredictedEpsGrowth,     // Předpokládaný růst EPS
        decimal RevenueGrowth,          // Růst tržeb
        decimal ProfitGrowth,           // Růst zisku
        decimal DividendGrowth,         // Růst dividend
        decimal PredictedFCFGrowth,     // Předpokládaný růst FCF
        decimal LongTermGrowthRate      // Dlouhodobý udržitelný růst
    );
}
