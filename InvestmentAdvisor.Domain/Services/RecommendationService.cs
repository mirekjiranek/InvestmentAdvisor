using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IValuationService _valuationService;

        public RecommendationService(IValuationService valuationService)
        {
            _valuationService = valuationService;
        }

        public Recommendation GenerateRecommendation(InvestmentInstrument instrument)
        {
            if (instrument.FundamentalData == null || instrument.PriceHistory.Count == 0)
            {
                throw new InvalidOperationException("Instrument requires fundamental and price data.");
            }

            var intrinsicValue = _valuationService.CalculateIntrinsicValue(instrument);
            var lastPrice = instrument.PriceHistory.OrderByDescending(p => p.Date).First().Close;

            // Procentuální rozdíl od tržní ceny
            // (intrinsicValue - lastPrice)/lastPrice * 100
            var diffPercent = ((intrinsicValue - lastPrice) / lastPrice) * 100m;

            // Rozhodovací logika:
            // >20%: StrongBuy
            // 10% až 20%: Buy
            // 0% až 10%: Accumulate
            // -5% až 0%: Hold
            // -10% až -5%: Reduce
            // -20% až -10%: Sell
            // < -20%: StrongSell

            RecommendationAction action;
            int score;
            if (diffPercent > 20)
            {
                action = RecommendationAction.StrongBuy;
                score = 1;
            }
            else if (diffPercent > 10)
            {
                action = RecommendationAction.Buy;
                score = 2;
            }
            else if (diffPercent > 0)
            {
                action = RecommendationAction.Accumulate;
                score = 3;
            }
            else if (diffPercent > -5)
            {
                action = RecommendationAction.Hold;
                score = 4;
            }
            else if (diffPercent > -10)
            {
                action = RecommendationAction.Reduce;
                score = 5;
            }
            else if (diffPercent > -20)
            {
                action = RecommendationAction.Sell;
                score = 6;
            }
            else
            {
                action = RecommendationAction.StrongSell;
                score = 7;
            }

            var timeHorizon = "12 měsíců";

            var targetPrice = intrinsicValue;

            var rationale = $"Vnitřní hodnota: {intrinsicValue:F2}, Aktuální cena: {lastPrice:F2}, Odchylka: {diffPercent:F2}%";

            // Úroveň rizika (pro zjednodušení):
            // pokud je Beta v fundamental data > 1.2 => Vysoké riziko,
            // 0.8 - 1.2 Střední riziko,
            // <0.8 Nízké riziko
            var beta = instrument.FundamentalData?.MarketRisk?.Beta ?? 1.0m;
            string riskLevel;
            if (beta > 1.2m) riskLevel = "Vysoké";
            else if (beta < 0.8m) riskLevel = "Nízké";
            else riskLevel = "Střední";

            return new Recommendation(
                action: action,
                score: score,
                timeHorizon: timeHorizon,
                targetPrice: targetPrice,
                rationale: rationale,
                riskLevel: riskLevel
            );
        }

        public async Task<Recommendation> GenerateRecommendationAsync(InvestmentInstrument instrument, CancellationToken cancellationToken = default)
        {
            // Asynchronní implementace, která deleguje na synchronní metodu
            // V reálném scénáři by tato metoda mohla provádět další asynchronní operace
            return await Task.Run(() => GenerateRecommendation(instrument), cancellationToken);
        }
    }
}