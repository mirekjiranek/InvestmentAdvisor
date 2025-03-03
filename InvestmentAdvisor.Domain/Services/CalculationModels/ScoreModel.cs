using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Skóre model hodnotí společnost v 6 kategoriích:
    /// 1. Valuace - je společnost podhodnocená či nadhodnocená? (P/E, EV/EBITDA, DCF, ...)
    /// 2. Růst - jak rychle roste firma v tržbách, ziscích, cash flow? (rychleji než sektor?)
    /// 3. Kvalita - jak kvalitní je podnikání? (ROE, ROA, marže, ...)
    /// 4. Finanční zdraví - jaká je finanční stabilita? (dluh, solventnost, likvidita, ...)
    /// 5. Riziko - jaké je riziko investice? (volatilita, beta, ...)
    /// 6. Sentiment - jak se k firmě staví analytici, instituce, ...? (upgrades/downgrades, insider buying, ...)
    /// 
    /// Výsledné skóre je vážený průměr těchto 6 kategorií, převedený na vnitřní hodnotu.
    /// </summary>
    public class ScoreModel
    {
        public decimal CalculateScore(FundamentalData data)
        {
            return CalculateScoreValue(data);
        }
        
        public decimal CalculateScoreValue(FundamentalData data)
        {
            // Váhy jednotlivých kategorií (mohou se lišit podle sektoru)
            decimal valuationWeight = 0.25m;
            decimal growthWeight = 0.20m;
            decimal qualityWeight = 0.20m;
            decimal financialHealthWeight = 0.15m;
            decimal riskWeight = 0.10m;
            decimal sentimentWeight = 0.10m;

            // 1. Valuační skóre (0-10, kde 10 = extrémně podhodnocené)
            decimal valuationScore = 0;
            int valuationFactorsCount = 0;

            // P/E ratio (nižší = lepší, ale příliš nízké může indikovat problémy)
            if (data.Valuation.PE > 0)
            {
                decimal peFactor = 0;
                
                if (data.Valuation.PE < 0.5m * data.Comparable.SectorAveragePE)
                    peFactor = 8; // Výrazně pod průměrem sektoru (může indikovat problémy)
                else if (data.Valuation.PE < 0.7m * data.Comparable.SectorAveragePE)
                    peFactor = 9; // Výrazně podhodnocené
                else if (data.Valuation.PE < 0.9m * data.Comparable.SectorAveragePE)
                    peFactor = 7; // Mírně podhodnocené
                else if (data.Valuation.PE < 1.1m * data.Comparable.SectorAveragePE)
                    peFactor = 5; // Blízko průměru sektoru
                else if (data.Valuation.PE < 1.3m * data.Comparable.SectorAveragePE)
                    peFactor = 3; // Mírně nadhodnocené
                else if (data.Valuation.PE < 1.5m * data.Comparable.SectorAveragePE)
                    peFactor = 2; // Nadhodnocené
                else
                    peFactor = 1; // Výrazně nadhodnocené
                
                valuationScore += peFactor;
                valuationFactorsCount++;
            }

            // EV/EBITDA (nižší = lepší)
            if (data.Valuation.EVEBITDA > 0 && data.Comparable.PeerEVEBITDA > 0)
            {
                decimal evEbitdaFactor = 0;
                
                if (data.Valuation.EVEBITDA < 0.6m * data.Comparable.PeerEVEBITDA)
                    evEbitdaFactor = 10; // Extrémně podhodnocené
                else if (data.Valuation.EVEBITDA < 0.8m * data.Comparable.PeerEVEBITDA)
                    evEbitdaFactor = 8; // Výrazně podhodnocené
                else if (data.Valuation.EVEBITDA < 0.95m * data.Comparable.PeerEVEBITDA)
                    evEbitdaFactor = 7; // Mírně podhodnocené
                else if (data.Valuation.EVEBITDA < 1.05m * data.Comparable.PeerEVEBITDA)
                    evEbitdaFactor = 5; // V souladu s trhem
                else if (data.Valuation.EVEBITDA < 1.2m * data.Comparable.PeerEVEBITDA)
                    evEbitdaFactor = 3; // Mírně nadhodnocené
                else if (data.Valuation.EVEBITDA < 1.5m * data.Comparable.PeerEVEBITDA)
                    evEbitdaFactor = 2; // Nadhodnocené
                else
                    evEbitdaFactor = 1; // Výrazně nadhodnocené
                
                valuationScore += evEbitdaFactor;
                valuationFactorsCount++;
            }

            // PEG ratio (blížící se 1.0 je ideální, nižší = podhodnocené)
            if (data.Valuation.PEG > 0)
            {
                decimal pegFactor = 0;
                
                if (data.Valuation.PEG < 0.7m)
                    pegFactor = 9; // Výrazně podhodnocené vzhledem k růstu
                else if (data.Valuation.PEG < 0.9m)
                    pegFactor = 8; // Mírně podhodnocené vzhledem k růstu
                else if (data.Valuation.PEG < 1.1m)
                    pegFactor = 6; // Férově oceněné vzhledem k růstu
                else if (data.Valuation.PEG < 1.5m)
                    pegFactor = 4; // Mírně nadhodnocené vzhledem k růstu
                else if (data.Valuation.PEG < 2.0m)
                    pegFactor = 2; // Nadhodnocené vzhledem k růstu
                else
                    pegFactor = 1; // Výrazně nadhodnocené vzhledem k růstu
                
                valuationScore += pegFactor;
                valuationFactorsCount++;
            }

            // Finalizace valuačního skóre
            if (valuationFactorsCount > 0)
            {
                valuationScore = valuationScore / valuationFactorsCount;
            }
            else
            {
                // Default hodnota pokud nemáme dostatek informací
                valuationScore = 5;
            }

            // 2. Růstové skóre (0-10, kde 10 = excelentní růst)
            decimal growthScore = 0;
            int growthFactorsCount = 0;

            // Růst tržeb
            if (data.Growth.RevenueGrowth != 0)
            {
                decimal revenueGrowthFactor = 0;
                
                if (data.Growth.RevenueGrowth > 0.25m)
                    revenueGrowthFactor = 10; // Excelentní růst
                else if (data.Growth.RevenueGrowth > 0.15m)
                    revenueGrowthFactor = 8; // Velmi dobrý růst
                else if (data.Growth.RevenueGrowth > 0.08m)
                    revenueGrowthFactor = 6; // Dobrý růst
                else if (data.Growth.RevenueGrowth > 0.03m)
                    revenueGrowthFactor = 5; // Průměrný růst
                else if (data.Growth.RevenueGrowth > 0)
                    revenueGrowthFactor = 3; // Slabý růst
                else if (data.Growth.RevenueGrowth > -0.05m)
                    revenueGrowthFactor = 2; // Mírný pokles
                else
                    revenueGrowthFactor = 1; // Výrazný pokles
                
                growthScore += revenueGrowthFactor;
                growthFactorsCount++;
            }

            // Růst zisků
            if (data.Growth.EarningsGrowth != 0)
            {
                decimal earningsGrowthFactor = 0;
                
                if (data.Growth.EarningsGrowth > 0.3m)
                    earningsGrowthFactor = 10; // Excelentní růst
                else if (data.Growth.EarningsGrowth > 0.2m)
                    earningsGrowthFactor = 8; // Velmi dobrý růst
                else if (data.Growth.EarningsGrowth > 0.1m)
                    earningsGrowthFactor = 6; // Dobrý růst
                else if (data.Growth.EarningsGrowth > 0.05m)
                    earningsGrowthFactor = 5; // Průměrný růst
                else if (data.Growth.EarningsGrowth > 0)
                    earningsGrowthFactor = 3; // Slabý růst
                else if (data.Growth.EarningsGrowth > -0.1m)
                    earningsGrowthFactor = 2; // Mírný pokles
                else
                    earningsGrowthFactor = 1; // Výrazný pokles
                
                growthScore += earningsGrowthFactor;
                growthFactorsCount++;
            }

            // Finalizace růstového skóre
            if (growthFactorsCount > 0)
            {
                growthScore = growthScore / growthFactorsCount;
            }
            else
            {
                // Default hodnota pokud nemáme dostatek informací
                growthScore = 5;
            }

            // 3. Skóre kvality (0-10, kde 10 = nejvyšší kvalita)
            decimal qualityScore = 0;
            int qualityFactorsCount = 0;

            // ROE
            if (data.Profitability.ROE != 0)
            {
                decimal roeFactor = 0;
                
                if (data.Profitability.ROE > 0.25m)
                    roeFactor = 10; // Excelentní ROE
                else if (data.Profitability.ROE > 0.20m)
                    roeFactor = 9; // Vynikající ROE
                else if (data.Profitability.ROE > 0.15m)
                    roeFactor = 8; // Velmi dobré ROE
                else if (data.Profitability.ROE > 0.12m)
                    roeFactor = 7; // Dobré ROE
                else if (data.Profitability.ROE > 0.10m)
                    roeFactor = 6; // Nadprůměrné ROE
                else if (data.Profitability.ROE > 0.08m)
                    roeFactor = 5; // Průměrné ROE
                else if (data.Profitability.ROE > 0.05m)
                    roeFactor = 4; // Podprůměrné ROE
                else if (data.Profitability.ROE > 0.02m)
                    roeFactor = 3; // Slabé ROE
                else if (data.Profitability.ROE > 0)
                    roeFactor = 2; // Velmi slabé ROE
                else
                    roeFactor = 1; // Negativní ROE
                
                qualityScore += roeFactor;
                qualityFactorsCount++;
            }

            // Čistá marže
            if (data.Profitability.NetMargin != 0)
            {
                decimal netMarginFactor = 0;
                
                if (data.Profitability.NetMargin > 0.25m)
                    netMarginFactor = 10; // Excelentní marže
                else if (data.Profitability.NetMargin > 0.20m)
                    netMarginFactor = 9; // Vynikající marže
                else if (data.Profitability.NetMargin > 0.15m)
                    netMarginFactor = 8; // Velmi dobrá marže
                else if (data.Profitability.NetMargin > 0.12m)
                    netMarginFactor = 7; // Dobrá marže
                else if (data.Profitability.NetMargin > 0.09m)
                    netMarginFactor = 6; // Nadprůměrná marže
                else if (data.Profitability.NetMargin > 0.06m)
                    netMarginFactor = 5; // Průměrná marže
                else if (data.Profitability.NetMargin > 0.04m)
                    netMarginFactor = 4; // Podprůměrná marže
                else if (data.Profitability.NetMargin > 0.02m)
                    netMarginFactor = 3; // Slabá marže
                else if (data.Profitability.NetMargin > 0)
                    netMarginFactor = 2; // Velmi slabá marže
                else
                    netMarginFactor = 1; // Negativní marže
                
                qualityScore += netMarginFactor;
                qualityFactorsCount++;
            }

            // Finalizace skóre kvality
            if (qualityFactorsCount > 0)
            {
                qualityScore = qualityScore / qualityFactorsCount;
            }
            else
            {
                // Default hodnota pokud nemáme dostatek informací
                qualityScore = 5;
            }

            // 4. Skóre finančního zdraví (0-10, kde 10 = nejlepší zdraví)
            decimal financialHealthScore = 0;
            int financialHealthFactorsCount = 0;

            // Debt/Equity poměr (nižší = lepší)
            if (data.Stability.DebtToEquity >= 0)
            {
                decimal debtToEquityFactor = 0;
                
                if (data.Stability.DebtToEquity < 0.1m)
                    debtToEquityFactor = 10; // Téměř žádný dluh
                else if (data.Stability.DebtToEquity < 0.3m)
                    debtToEquityFactor = 9; // Velmi nízký dluh
                else if (data.Stability.DebtToEquity < 0.5m)
                    debtToEquityFactor = 8; // Nízký dluh
                else if (data.Stability.DebtToEquity < 0.7m)
                    debtToEquityFactor = 7; // Přiměřený dluh
                else if (data.Stability.DebtToEquity < 1.0m)
                    debtToEquityFactor = 6; // Mírně zvýšený dluh
                else if (data.Stability.DebtToEquity < 1.5m)
                    debtToEquityFactor = 5; // Průměrný dluh
                else if (data.Stability.DebtToEquity < 2.0m)
                    debtToEquityFactor = 4; // Zvýšený dluh
                else if (data.Stability.DebtToEquity < 3.0m)
                    debtToEquityFactor = 3; // Vysoký dluh
                else if (data.Stability.DebtToEquity < 5.0m)
                    debtToEquityFactor = 2; // Velmi vysoký dluh
                else
                    debtToEquityFactor = 1; // Extrémně vysoký dluh
                
                financialHealthScore += debtToEquityFactor;
                financialHealthFactorsCount++;
            }

            // Current Ratio (likvidita) - vyšší = lepší
            if (data.Stability.CurrentRatio > 0)
            {
                decimal currentRatioFactor = 0;
                
                if (data.Stability.CurrentRatio > 3.0m)
                    currentRatioFactor = 10; // Excelentní likvidita (může být až příliš)
                else if (data.Stability.CurrentRatio > 2.5m)
                    currentRatioFactor = 9; // Velmi silná likvidita
                else if (data.Stability.CurrentRatio > 2.0m)
                    currentRatioFactor = 8; // Silná likvidita
                else if (data.Stability.CurrentRatio > 1.5m)
                    currentRatioFactor = 7; // Dobrá likvidita
                else if (data.Stability.CurrentRatio > 1.2m)
                    currentRatioFactor = 6; // Adekvátní likvidita
                else if (data.Stability.CurrentRatio > 1.0m)
                    currentRatioFactor = 5; // Přijatelná likvidita
                else if (data.Stability.CurrentRatio > 0.8m)
                    currentRatioFactor = 4; // Mírně problematická likvidita
                else if (data.Stability.CurrentRatio > 0.6m)
                    currentRatioFactor = 3; // Problematická likvidita
                else if (data.Stability.CurrentRatio > 0.4m)
                    currentRatioFactor = 2; // Velmi nízká likvidita
                else
                    currentRatioFactor = 1; // Kriticky nízká likvidita
                
                financialHealthScore += currentRatioFactor;
                financialHealthFactorsCount++;
            }

            // Finalizace skóre finančního zdraví
            if (financialHealthFactorsCount > 0)
            {
                financialHealthScore = financialHealthScore / financialHealthFactorsCount;
            }
            else
            {
                // Default hodnota pokud nemáme dostatek informací
                financialHealthScore = 5;
            }

            // 5. Rizikové skóre (0-10, kde 10 = nejnižší riziko)
            decimal riskScore = 0;
            int riskFactorsCount = 0;

            // Beta (nižší = nižší volatilita = nižší riziko)
            if (data.MarketRisk.Beta > 0)
            {
                decimal betaFactor = 0;
                
                if (data.MarketRisk.Beta < 0.5m)
                    betaFactor = 10; // Extrémně nízká volatilita
                else if (data.MarketRisk.Beta < 0.75m)
                    betaFactor = 9; // Velmi nízká volatilita
                else if (data.MarketRisk.Beta < 0.9m)
                    betaFactor = 8; // Nízká volatilita
                else if (data.MarketRisk.Beta < 1.0m)
                    betaFactor = 7; // Podprůměrná volatilita
                else if (data.MarketRisk.Beta < 1.1m)
                    betaFactor = 6; // Průměrná volatilita
                else if (data.MarketRisk.Beta < 1.25m)
                    betaFactor = 5; // Mírně nadprůměrná volatilita
                else if (data.MarketRisk.Beta < 1.5m)
                    betaFactor = 4; // Nadprůměrná volatilita
                else if (data.MarketRisk.Beta < 1.75m)
                    betaFactor = 3; // Vysoká volatilita
                else if (data.MarketRisk.Beta < 2.0m)
                    betaFactor = 2; // Velmi vysoká volatilita
                else
                    betaFactor = 1; // Extrémně vysoká volatilita
                
                riskScore += betaFactor;
                riskFactorsCount++;
            }

            // Finalizace rizikového skóre
            if (riskFactorsCount > 0)
            {
                riskScore = riskScore / riskFactorsCount;
            }
            else
            {
                // Default hodnota pokud nemáme dostatek informací
                riskScore = 5;
            }

            // 6. Skóre sentimentu (0-10, kde 10 = nejpozitivnější sentiment)
            decimal sentimentScore = 0;
            int sentimentFactorsCount = 0;

            // Analýzy analytiků
            if (data.Sentiment.AnalystConsensus > 0)
            {
                decimal analystConsenusFactor = 0;
                
                if (data.Sentiment.AnalystConsensus > 4.5m)
                    analystConsenusFactor = 10; // Silný nákup
                else if (data.Sentiment.AnalystConsensus > 4.0m)
                    analystConsenusFactor = 9; // Nákup
                else if (data.Sentiment.AnalystConsensus > 3.5m)
                    analystConsenusFactor = 8; // Mírný nákup
                else if (data.Sentiment.AnalystConsensus > 3.0m)
                    analystConsenusFactor = 7; // Slabý nákup
                else if (data.Sentiment.AnalystConsensus > 2.5m)
                    analystConsenusFactor = 5; // Neutrální
                else if (data.Sentiment.AnalystConsensus > 2.0m)
                    analystConsenusFactor = 3; // Slabý prodej
                else if (data.Sentiment.AnalystConsensus > 1.5m)
                    analystConsenusFactor = 2; // Prodej
                else
                    analystConsenusFactor = 1; // Silný prodej
                
                sentimentScore += analystConsenusFactor;
                sentimentFactorsCount++;
            }

            // Finalizace skóre sentimentu
            if (sentimentFactorsCount > 0)
            {
                sentimentScore = sentimentScore / sentimentFactorsCount;
            }
            else
            {
                // Default hodnota pokud nemáme dostatek informací
                sentimentScore = 5;
            }

            // Výpočet konečného skóre jako váženého průměru
            decimal finalScore = 
                (valuationScore * valuationWeight) +
                (growthScore * growthWeight) +
                (qualityScore * qualityWeight) +
                (financialHealthScore * financialHealthWeight) +
                (riskScore * riskWeight) +
                (sentimentScore * sentimentWeight);

            // Převedení skóre na vnitřní hodnotu
            decimal lastPrice = 0;
            if (data.PriceMetrics != null && data.PriceMetrics.Price > 0)
            {
                lastPrice = data.PriceMetrics.Price;
            }
            else
            {
                lastPrice = data.Price;
            }

            // Převod skóre na cenové rozpětí:
            // 5.0 = férová hodnota = aktuální cena
            // 10.0 = +100% potenciál
            // 0.0 = -50% potenciál
            decimal priceAdjustment = (finalScore - 5.0m) / 5.0m;
            decimal fairValuePrice = lastPrice * (1.0m + priceAdjustment);

            return fairValuePrice;
        }
    }
}