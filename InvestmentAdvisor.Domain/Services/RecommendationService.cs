using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            // Vypočítáme vnitřní hodnotu
            var intrinsicValue = _valuationService.CalculateIntrinsicValue(instrument);
            
            // Získáme aktuální tržní cenu
            var lastPrice = instrument.PriceHistory.OrderByDescending(p => p.Date).First().Close;

            // Procentuální rozdíl od tržní ceny (potenciál růstu/poklesu)
            var diffPercent = ((intrinsicValue - lastPrice) / lastPrice) * 100m;

            // Získáme další faktory pro rozhodování
            FundamentalData data = instrument.FundamentalData;
            
            // Aktualizujeme cenu ve fundamentálních datech pro výpočty
            data.Price = lastPrice;
            
            // Ověříme kvalitu firmy pro upřesnění doporučení
            bool isHighQualityFirm = data.Profitability.ROE > 0.15m && 
                                    data.Stability.DebtToEquity < 1.0m && 
                                    data.Stability.InterestCoverage > 5.0m;
            
            // Ověříme momentum - roste firma nebo klesá?
            bool hasPositiveMomentum = data.Growth.PredictedEpsGrowth > 0.10m &&
                                      data.Growth.SalesGrowth > 0.05m;
            
            // Ověříme valuaci pomocí více metrik než jen DCF
            bool isUndervalued = diffPercent > 10m &&
                                data.Valuation.PE < data.Comparable.SectorAveragePE * 0.85m &&
                                data.Valuation.EvEbitda < data.Comparable.PeerEVEBITDA * 0.9m;
            
            bool isSeverlyOvervalued = diffPercent < -20m &&
                                     data.Valuation.PE > data.Comparable.SectorAveragePE * 1.5m;
            
            // Ověříme riziko
            bool isHighRisk = data.MarketRisk.Beta > 1.5m || 
                             data.MarketRisk.Volatility > 0.3m || 
                             data.Stability.DebtToEquity > 2.0m;
            
            bool isLowRisk = data.MarketRisk.Beta < 0.8m && 
                            data.Stability.DebtToEquity < 0.5m;
            
            // VYLEPŠENÁ ROZHODOVACÍ LOGIKA
            // Procentuální hodnocení je výchozí, ale modifikujeme dle dalších faktorů

            RecommendationAction action;
            int score;
            
            // Základní doporučení dle procentuálního rozdílu
            if (diffPercent > 30)
            {
                action = RecommendationAction.StrongBuy;
                score = 1;
            }
            else if (diffPercent > 15)
            {
                action = RecommendationAction.Buy;
                score = 2;
            }
            else if (diffPercent > 5)
            {
                action = RecommendationAction.Accumulate;
                score = 3;
            }
            else if (diffPercent > -5)
            {
                action = RecommendationAction.Hold;
                score = 4;
            }
            else if (diffPercent > -15)
            {
                action = RecommendationAction.Reduce;
                score = 5;
            }
            else if (diffPercent > -30)
            {
                action = RecommendationAction.Sell;
                score = 6;
            }
            else
            {
                action = RecommendationAction.StrongSell;
                score = 7;
            }

            // Úpravy doporučení na základě dodatečných faktorů
            
            // Vysoce kvalitní firmy s pozitivním momentum - silnější nákupní signál
            if (isHighQualityFirm && hasPositiveMomentum && diffPercent > 0)
            {
                if (action == RecommendationAction.Accumulate)
                {
                    action = RecommendationAction.Buy;
                    score = 2;
                }
                else if (action == RecommendationAction.Hold && diffPercent > -3m)
                {
                    action = RecommendationAction.Accumulate;
                    score = 3;
                }
            }
            
            // Podhodnocené firmy s nízkým rizikem - silnější nákupní signál
            if (isUndervalued && isLowRisk)
            {
                if (action == RecommendationAction.Buy)
                {
                    action = RecommendationAction.StrongBuy;
                    score = 1;
                }
                else if (action == RecommendationAction.Accumulate)
                {
                    action = RecommendationAction.Buy;
                    score = 2;
                }
            }
            
            // Vysoce rizikové firmy - opatrnější přístup
            if (isHighRisk)
            {
                if (action == RecommendationAction.StrongBuy)
                {
                    action = RecommendationAction.Buy;
                    score = 2;
                }
                else if (action == RecommendationAction.Buy)
                {
                    action = RecommendationAction.Accumulate;
                    score = 3;
                }
            }
            
            // Vážně nadhodnocené firmy - silnější prodejní signál
            if (isSeverlyOvervalued)
            {
                if (action == RecommendationAction.Reduce)
                {
                    action = RecommendationAction.Sell;
                    score = 6;
                }
                else if (action == RecommendationAction.Sell && diffPercent < -25m)
                {
                    action = RecommendationAction.StrongSell;
                    score = 7;
                }
            }

            // Nastavení časového horizontu podle typu doporučení a volatility
            string timeHorizon;
            if (data.MarketRisk.Volatility > 0.4m || action == RecommendationAction.StrongBuy || action == RecommendationAction.StrongSell)
            {
                timeHorizon = "6 měsíců";
            }
            else if (isHighQualityFirm && (action == RecommendationAction.Buy || action == RecommendationAction.Accumulate))
            {
                timeHorizon = "24 měsíců";
            }
            else
            {
                timeHorizon = "12 měsíců";
            }

            // Cílová cena včetně úprav pro riziko
            decimal targetPrice = intrinsicValue;
            if (isHighRisk)
            {
                // Konzervativnější cílová cena pro rizikovější instrumenty
                targetPrice = lastPrice + (intrinsicValue - lastPrice) * 0.7m;
            }

            // Podrobnější zdůvodnění doporučení
            var reasonsList = new List<string>();
            
            // Základ zdůvodnění - vnitřní hodnota vs. aktuální cena
            reasonsList.Add($"Vnitřní hodnota: {intrinsicValue:F2}, Aktuální cena: {lastPrice:F2}, Odchylka: {diffPercent:F2}%");
            
            // Růstový potenciál
            if (data.Growth.PredictedEpsGrowth > 0.15m)
                reasonsList.Add($"Vysoký očekávaný růst zisků ({data.Growth.PredictedEpsGrowth * 100m:F1}% ročně)");
                
            // Kvalita
            if (isHighQualityFirm)
                reasonsList.Add($"Kvalitní fundamenty (ROE: {data.Profitability.ROE * 100m:F1}%, nízké zadlužení)");
                
            // Valuace
            if (isUndervalued)
                reasonsList.Add($"Atraktivní valuace (P/E: {data.Valuation.PE:F1}x vs. sektor: {data.Comparable.SectorAveragePE:F1}x)");
            else if (isSeverlyOvervalued)
                reasonsList.Add($"Vysoká valuace (P/E: {data.Valuation.PE:F1}x vs. sektor: {data.Comparable.SectorAveragePE:F1}x)");
                
            // Riziko
            if (isHighRisk)
                reasonsList.Add($"Zvýšené riziko (Beta: {data.MarketRisk.Beta:F2}, Zadlužení: {data.Stability.DebtToEquity:F2}x)");
            else if (isLowRisk)
                reasonsList.Add($"Nízké riziko (Beta: {data.MarketRisk.Beta:F2}, Zadlužení: {data.Stability.DebtToEquity:F2}x)");
                
            // Sentiment
            if (data.Sentiment.AnalystConsensus > 0.7m)
                reasonsList.Add("Pozitivní sentiment analytiků");
            else if (data.Sentiment.AnalystConsensus < -0.3m)
                reasonsList.Add("Negativní sentiment analytiků");
                
            // Spojíme důvody do jednoho textu
            var rationale = string.Join(". ", reasonsList);

            // Úroveň rizika - podrobnější vyhodnocení
            var beta = data.MarketRisk.Beta;
            var debtToEquity = data.Stability.DebtToEquity;
            var volatility = data.MarketRisk.Volatility;
            
            string riskLevel;
            if ((beta > 1.5m && volatility > 0.3m) || debtToEquity > 2.0m) riskLevel = "Velmi vysoké";
            else if (beta > 1.2m || debtToEquity > 1.2m) riskLevel = "Vysoké";
            else if (beta > 0.8m && beta <= 1.2m && debtToEquity <= 1.2m) riskLevel = "Střední";
            else if (beta > 0.6m || debtToEquity > 0.5m) riskLevel = "Nízké";
            else riskLevel = "Velmi nízké";

            // Vytvoření finálního doporučení s podrobnějšími informacemi
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