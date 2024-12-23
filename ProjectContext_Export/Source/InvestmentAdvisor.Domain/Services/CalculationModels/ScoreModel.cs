using Domain.Entities;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Skórovací model:
    /// Ohodnotí firmu na základě několika oblastí: 
    /// - Valuace (levná vs. drahá)
    /// - Růst (vysoký růst EPS/FCF)
    /// - Profitabilita (vysoká marže, ROE)
    /// - Stabilita (nízké zadlužení, dobré krytí úroků)
    /// - Dividendová politika (stabilní, rostoucí)
    /// - Riziko (Beta, volatilita)
    /// 
    /// Z každé oblasti vypočte dílčí skóre a pak je zprůměruje podle předem daných vah.
    /// </summary>
    public class ScoreModel
    {
        public decimal CalculateScoreValue(FundamentalData data)
        {
            // Příklad váh:
            decimal valuationWeight = 0.2m;
            decimal growthWeight = 0.2m;
            decimal profitabilityWeight = 0.2m;
            decimal stabilityWeight = 0.15m;
            decimal dividendWeight = 0.15m;
            decimal riskWeight = 0.1m;

            // Jednoduché ohodnocení: 
            // Valuation: Nižší PE => lepší skóre
            decimal valuationScore = 100m - data.Valuation.PE * 2m;

            // Growth: Vyšší EPS growth => vyšší skóre
            decimal growthScore = 100m * (1 + data.Growth.PredictedEpsGrowth);

            // Profitabilita: Vysoký ROE a marže => vyšší skóre
            decimal profitabilityScore = (data.Profitability.ROE * 10m) + (data.Profitability.NetMargin * 50m);

            // Stabilita: Nízké Debt/Equity, vysoké Interest coverage => vyšší skóre
            decimal stabilityScore = 100m - data.Stability.DebtToEquity * 10m + data.Stability.InterestCoverage * 5m;

            // Dividend: Pokud firma roste dividendu a má slušný výnos, zvyšuje skóre
            decimal dividendScore = (data.Dividend.DividendYield * 20m) + (data.Dividend.DividendGrowth * 50m);

            // Riziko: Nižší Beta => lepší skóre
            decimal riskScore = 100m - (data.MarketRisk.Beta * 20m);

            // Celkové skóre jako vážený průměr
            decimal totalScore =
                (valuationScore * valuationWeight) +
                (growthScore * growthWeight) +
                (profitabilityScore * profitabilityWeight) +
                (stabilityScore * stabilityWeight) +
                (dividendScore * dividendWeight) +
                (riskScore * riskWeight);

            return totalScore;
        }
    }
}
