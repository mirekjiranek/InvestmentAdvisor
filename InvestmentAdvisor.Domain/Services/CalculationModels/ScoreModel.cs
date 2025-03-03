using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšený skóre model hodnotí společnost v 8 kategoriích:
    /// 1. Valuace - je společnost podhodnocená či nadhodnocená? (P/E, EV/EBITDA, DCF, ...)
    /// 2. Růst - jak rychle roste firma v tržbách, ziscích, cash flow? (rychleji než sektor?)
    /// 3. Kvalita - jak kvalitní je podnikání? (ROE, ROA, marže, ...)
    /// 4. Finanční zdraví - jaká je finanční stabilita? (dluh, solventnost, likvidita, ...)
    /// 5. Riziko - jaké je riziko investice? (volatilita, beta, ...)
    /// 6. Sentiment - jak se k firmě staví analytici, instituce, ...? (upgrades/downgrades, insider buying, ...)
    /// 7. Likvidita - jak likvidní je akcie? (objem obchodů, bid-ask spread, ...)
    /// 8. ESG faktory - jak společnost přistupuje k environemntálním, sociálním a governance otázkám?
    /// 
    /// Výsledné skóre je vážený průměr těchto kategorií, převedený na vnitřní hodnotu.
    /// Model používá nelineární skórovací funkce pro některé metriky a přizpůsobuje váhy
    /// podle sektoru a typu společnosti.
    /// </summary>
    public class ScoreModel
    {
        public decimal CalculateScore(FundamentalData data)
        {
            return CalculateScoreValue(data);
        }
        
        public decimal CalculateScoreValue(FundamentalData data)
        {
            // Určení sektoru a typu společnosti pro dynamické přizpůsobení vah
            string sector = DetermineSector(data);
            string companyType = DetermineCompanyType(data);
            
            // Získání kalibrovaných vah podle sektoru a typu společnosti
            Dictionary<string, decimal> weights = GetCalibratedWeights(sector, companyType);
            
            // Výpočet jednotlivých složek skóre
            Dictionary<string, decimal> scores = new Dictionary<string, decimal>();
            
            // 1. Valuační skóre (0-10, kde 10 = extrémně podhodnocené)
            scores["Valuation"] = CalculateValuationScore(data);
            
            // 2. Růstové skóre (0-10, kde 10 = excelentní růst)
            scores["Growth"] = CalculateGrowthScore(data);
            
            // 3. Skóre kvality (0-10, kde 10 = nejvyšší kvalita)
            scores["Quality"] = CalculateQualityScore(data);
            
            // 4. Skóre finančního zdraví (0-10, kde 10 = nejlepší zdraví)
            scores["FinancialHealth"] = CalculateFinancialHealthScore(data);
            
            // 5. Rizikové skóre (0-10, kde 10 = nejnižší riziko)
            scores["Risk"] = CalculateRiskScore(data);
            
            // 6. Skóre sentimentu (0-10, kde 10 = nejpozitivnější sentiment)
            scores["Sentiment"] = CalculateSentimentScore(data);
            
            // 7. Skóre likvidity (0-10, kde 10 = nejlepší likvidita)
            scores["Liquidity"] = CalculateLiquidityScore(data);
            
            // 8. ESG skóre (0-10, kde 10 = nejlepší ESG výkon)
            scores["ESG"] = CalculateESGScore(data);
            
            // Výpočet konečného skóre jako váženého průměru
            decimal finalScore = 0;
            decimal totalWeight = 0;
            
            foreach (var category in scores.Keys)
            {
                if (weights.ContainsKey(category))
                {
                    finalScore += scores[category] * weights[category];
                    totalWeight += weights[category];
                }
            }
            
            // Normalizace skóre
            if (totalWeight > 0)
            {
                finalScore /= totalWeight;
            }
            else
            {
                finalScore = 5.0m; // Neutrální skóre, pokud nemáme žádné váhy
            }
            
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
            
            // Použití nelineární funkce pro převod skóre na cenové rozpětí:
            // 5.0 = férová hodnota = aktuální cena
            // 10.0 = +100% potenciál (pro skóre 10 - silně podhodnocené)
            // 7.5 = +50% potenciál (pro skóre 7.5 - mírně podhodnocené)
            // 2.5 = -25% potenciál (pro skóre 2.5 - mírně nadhodnocené)
            // 0.0 = -50% potenciál (pro skóre 0 - silně nadhodnocené)
            decimal priceAdjustment = CalculateNonlinearPriceAdjustment(finalScore);
            decimal fairValuePrice = lastPrice * (1.0m + priceAdjustment);
            
            return fairValuePrice;
        }
        
        /// <summary>
        /// Určí sektor společnosti na základě dostupných dat
        /// </summary>
        private string DetermineSector(FundamentalData data)
        {
            // V reálné implementaci by toto bylo určeno z metadat společnosti
            // Pro demonstraci používáme zjednodušené určení na základě charakteristik
            
            if (data.Profitability.NetMargin > 0.2m && data.Stability.DebtToEquity < 0.5m)
                return "Technology";
            else if (data.Profitability.NetMargin > 0.15m && data.Dividend.DividendYield > 0.03m)
                return "Consumer";
            else if (data.Stability.DebtToEquity > 1.5m && data.Dividend.DividendYield > 0.04m)
                return "Utilities";
            else if (data.Stability.DebtToEquity > 1.2m && data.Profitability.NetMargin < 0.08m)
                return "Industrial";
            else if (data.Profitability.NetMargin > 0.25m && data.Stability.CurrentRatio > 2.0m)
                return "Healthcare";
            else if (data.Stability.DebtToEquity > 2.0m)
                return "Financial";
            else
                return "Other";
        }
        
        /// <summary>
        /// Určí typ společnosti na základě jejích charakteristik
        /// </summary>
        private string DetermineCompanyType(FundamentalData data)
        {
            // Určení typu společnosti na základě růstu, výnosů a stability
            
            if (data.Growth.RevenueGrowth > 0.25m && data.Growth.EarningsGrowth > 0.3m)
                return "HighGrowth";
            else if (data.Earnings.EPS <= 0)
                return "Unprofitable";
            else if (data.Valuation.PE < 10.0m && data.Valuation.PB < 1.0m)
                return "Value";
            else if (data.Dividend.DividendYield > 0.04m && data.Growth.RevenueGrowth < 0.05m)
                return "Income";
            else if (data.MarketRisk.Beta < 0.8m && data.Stability.DebtToEquity < 0.5m)
                return "Defensive";
            else if (data.MarketRisk.Beta > 1.5m && data.Growth.RevenueGrowth > 0.15m)
                return "Aggressive";
            else
                return "Balanced";
        }
        
        /// <summary>
        /// Poskytuje kalibrované váhy pro jednotlivé kategorie skóre
        /// podle sektoru a typu společnosti
        /// </summary>
        private Dictionary<string, decimal> GetCalibratedWeights(string sector, string companyType)
        {
            // Výchozí váhy, které budou upraveny podle sektoru a typu
            var weights = new Dictionary<string, decimal>
            {
                { "Valuation", 0.20m },
                { "Growth", 0.15m },
                { "Quality", 0.15m },
                { "FinancialHealth", 0.15m },
                { "Risk", 0.15m },
                { "Sentiment", 0.10m },
                { "Liquidity", 0.05m },
                { "ESG", 0.05m }
            };
            
            // Úpravy dle sektoru
            switch (sector)
            {
                case "Technology":
                    weights["Growth"] += 0.05m;
                    weights["FinancialHealth"] -= 0.05m;
                    break;
                case "Utilities":
                    weights["Growth"] -= 0.05m;
                    weights["FinancialHealth"] += 0.05m;
                    break;
                case "Healthcare":
                    weights["Growth"] += 0.05m;
                    weights["ESG"] += 0.05m;
                    weights["Sentiment"] -= 0.05m;
                    weights["Liquidity"] -= 0.05m;
                    break;
                case "Financial":
                    weights["FinancialHealth"] += 0.10m;
                    weights["Valuation"] -= 0.05m;
                    weights["ESG"] -= 0.05m;
                    break;
                case "Consumer":
                    weights["Quality"] += 0.05m;
                    weights["Sentiment"] += 0.05m;
                    weights["Risk"] -= 0.05m;
                    weights["Liquidity"] -= 0.05m;
                    break;
            }
            
            // Úpravy dle typu společnosti
            switch (companyType)
            {
                case "HighGrowth":
                    weights["Growth"] += 0.10m;
                    weights["Valuation"] -= 0.05m;
                    weights["FinancialHealth"] -= 0.05m;
                    break;
                case "Unprofitable":
                    weights["Growth"] += 0.10m;
                    weights["Quality"] -= 0.10m;
                    weights["FinancialHealth"] += 0.05m;
                    weights["Risk"] += 0.05m;
                    weights["Valuation"] -= 0.10m;
                    break;
                case "Value":
                    weights["Valuation"] += 0.10m;
                    weights["Growth"] -= 0.05m;
                    weights["Sentiment"] -= 0.05m;
                    break;
                case "Income":
                    weights["FinancialHealth"] += 0.05m;
                    weights["Growth"] -= 0.10m;
                    weights["Risk"] -= 0.05m;
                    weights["Quality"] += 0.10m;
                    break;
                case "Defensive":
                    weights["Risk"] += 0.10m;
                    weights["FinancialHealth"] += 0.05m;
                    weights["Growth"] -= 0.10m;
                    weights["Sentiment"] -= 0.05m;
                    break;
                case "Aggressive":
                    weights["Growth"] += 0.10m;
                    weights["Risk"] -= 0.05m;
                    weights["Sentiment"] += 0.05m;
                    weights["FinancialHealth"] -= 0.10m;
                    break;
            }
            
            // Normalizace vah (součet = 1.0)
            decimal totalWeight = 0;
            foreach (var weight in weights.Values)
            {
                totalWeight += weight;
            }
            
            if (totalWeight > 0)
            {
                foreach (var key in weights.Keys.ToArray())
                {
                    weights[key] /= totalWeight;
                }
            }
            
            return weights;
        }
        
        /// <summary>
        /// Vypočítá valuační skóre na základě různých valuačních metrik
        /// </summary>
        private decimal CalculateValuationScore(FundamentalData data)
        {
            // Váhy jednotlivých valuačních metrik
            Dictionary<string, decimal> metricWeights = new Dictionary<string, decimal>
            {
                { "PE", 0.25m },
                { "EVEBITDA", 0.25m },
                { "PEG", 0.20m },
                { "PS", 0.15m },
                { "PB", 0.15m }
            };
            
            Dictionary<string, decimal> metricScores = new Dictionary<string, decimal>();
            
            // P/E ratio - nelineární skórovací funkce
            if (data.Valuation.PE > 0 && data.Comparable.SectorAveragePE > 0)
            {
                decimal peRatio = data.Valuation.PE / data.Comparable.SectorAveragePE;
                
                // Nelineární skórovací funkce: vysoké skóre pro PE výrazně pod průměrem sektoru,
                // středně vysoké pro hodnoty blízko průměru, nízké pro hodnoty výrazně nad průměrem
                decimal peScore;
                
                if (peRatio < 0.6m)
                    peScore = 9.0m + ((0.6m - peRatio) / 0.6m); // 9-10 pro PE < 60% průměru sektoru
                else if (peRatio < 0.8m)
                    peScore = 7.0m + ((0.8m - peRatio) / 0.2m) * 2.0m; // 7-9 pro PE 60-80% průměru
                else if (peRatio < 1.0m)
                    peScore = 5.0m + ((1.0m - peRatio) / 0.2m) * 2.0m; // 5-7 pro PE 80-100% průměru
                else if (peRatio < 1.2m)
                    peScore = 3.0m + ((1.2m - peRatio) / 0.2m) * 2.0m; // 3-5 pro PE 100-120% průměru
                else if (peRatio < 1.5m)
                    peScore = 1.0m + ((1.5m - peRatio) / 0.3m) * 2.0m; // 1-3 pro PE 120-150% průměru
                else
                    peScore = Math.Max(0, 1.0m - ((peRatio - 1.5m) / 1.5m)); // 0-1 pro PE > 150% průměru
                
                metricScores["PE"] = peScore;
            }
            
            // EV/EBITDA (nelineární funkce)
            if (data.Valuation.EVEBITDA > 0 && data.Comparable.PeerEVEBITDA > 0)
            {
                decimal evEbitdaRatio = data.Valuation.EVEBITDA / data.Comparable.PeerEVEBITDA;
                
                decimal evEbitdaScore;
                
                if (evEbitdaRatio < 0.7m)
                    evEbitdaScore = 9.0m + ((0.7m - evEbitdaRatio) / 0.7m); // 9-10 pro výrazně nízké hodnoty
                else if (evEbitdaRatio < 0.9m)
                    evEbitdaScore = 7.0m + ((0.9m - evEbitdaRatio) / 0.2m) * 2.0m;
                else if (evEbitdaRatio < 1.1m)
                    evEbitdaScore = 5.0m + ((1.1m - evEbitdaRatio) / 0.2m) * 2.0m;
                else if (evEbitdaRatio < 1.3m)
                    evEbitdaScore = 3.0m + ((1.3m - evEbitdaRatio) / 0.2m) * 2.0m;
                else if (evEbitdaRatio < 1.6m)
                    evEbitdaScore = 1.0m + ((1.6m - evEbitdaRatio) / 0.3m) * 2.0m;
                else
                    evEbitdaScore = Math.Max(0, 1.0m - ((evEbitdaRatio - 1.6m) / 1.6m));
                
                metricScores["EVEBITDA"] = evEbitdaScore;
            }
            
            // PEG ratio (blížící se 1.0 je ideální, nižší = podhodnocené)
            if (data.Valuation.PEG > 0)
            {
                decimal pegScore;
                
                if (data.Valuation.PEG < 0.7m)
                    pegScore = 8.0m + ((0.7m - data.Valuation.PEG) / 0.7m) * 2.0m;
                else if (data.Valuation.PEG < 1.0m)
                    pegScore = 6.0m + ((1.0m - data.Valuation.PEG) / 0.3m) * 2.0m;
                else if (data.Valuation.PEG < 1.5m)
                    pegScore = 4.0m + ((1.5m - data.Valuation.PEG) / 0.5m) * 2.0m;
                else if (data.Valuation.PEG < 2.0m)
                    pegScore = 2.0m + ((2.0m - data.Valuation.PEG) / 0.5m) * 2.0m;
                else
                    pegScore = Math.Max(0, 2.0m - ((data.Valuation.PEG - 2.0m) / 1.0m));
                
                metricScores["PEG"] = pegScore;
            }
            
            // Price/Sales
            if (data.Valuation.PS > 0 && data.Comparable.SectorPriceSales > 0)
            {
                decimal psRatio = data.Valuation.PS / data.Comparable.SectorPriceSales;
                
                decimal psScore;
                
                if (psRatio < 0.6m)
                    psScore = 9.0m + ((0.6m - psRatio) / 0.6m);
                else if (psRatio < 0.8m)
                    psScore = 7.0m + ((0.8m - psRatio) / 0.2m) * 2.0m;
                else if (psRatio < 1.0m)
                    psScore = 5.0m + ((1.0m - psRatio) / 0.2m) * 2.0m;
                else if (psRatio < 1.2m)
                    psScore = 3.0m + ((1.2m - psRatio) / 0.2m) * 2.0m;
                else if (psRatio < 1.5m)
                    psScore = 1.0m + ((1.5m - psRatio) / 0.3m) * 2.0m;
                else
                    psScore = Math.Max(0, 1.0m - ((psRatio - 1.5m) / 1.5m));
                
                metricScores["PS"] = psScore;
            }
            
            // Price/Book
            if (data.Valuation.PB > 0 && data.Comparable.SectorPriceBook > 0)
            {
                decimal pbRatio = data.Valuation.PB / data.Comparable.SectorPriceBook;
                
                decimal pbScore;
                
                if (pbRatio < 0.6m)
                    pbScore = 9.0m + ((0.6m - pbRatio) / 0.6m);
                else if (pbRatio < 0.8m)
                    pbScore = 7.0m + ((0.8m - pbRatio) / 0.2m) * 2.0m;
                else if (pbRatio < 1.0m)
                    pbScore = 5.0m + ((1.0m - pbRatio) / 0.2m) * 2.0m;
                else if (pbRatio < 1.2m)
                    pbScore = 3.0m + ((1.2m - pbRatio) / 0.2m) * 2.0m;
                else if (pbRatio < 1.5m)
                    pbScore = 1.0m + ((1.5m - pbRatio) / 0.3m) * 2.0m;
                else
                    pbScore = Math.Max(0, 1.0m - ((pbRatio - 1.5m) / 1.5m));
                
                metricScores["PB"] = pbScore;
            }
            
            // Vážený průměr valuačních skóre
            decimal totalValuationScore = 0;
            decimal appliedWeight = 0;
            
            foreach (var metric in metricScores.Keys)
            {
                if (metricWeights.ContainsKey(metric))
                {
                    totalValuationScore += metricScores[metric] * metricWeights[metric];
                    appliedWeight += metricWeights[metric];
                }
            }
            
            // Pokud nemáme dostatek valuačních metrik, vrátíme neutrální hodnocení
            if (appliedWeight > 0)
                return totalValuationScore / appliedWeight;
            else
                return 5.0m;
        }
        
        /// <summary>
        /// Vypočítá růstové skóre na základě různých růstových metrik
        /// </summary>
        private decimal CalculateGrowthScore(FundamentalData data)
        {
            Dictionary<string, decimal> metricWeights = new Dictionary<string, decimal>
            {
                { "RevenueGrowth", 0.3m },
                { "EarningsGrowth", 0.4m },
                { "FCFGrowth", 0.3m }
            };
            
            Dictionary<string, decimal> metricScores = new Dictionary<string, decimal>();
            
            // Růst tržeb
            if (data.Growth.RevenueGrowth != 0)
            {
                decimal revenueGrowthScore = NonlinearGrowthScore(data.Growth.RevenueGrowth, 0.03m, 0.25m);
                metricScores["RevenueGrowth"] = revenueGrowthScore;
            }
            
            // Růst zisků
            if (data.Growth.EarningsGrowth != 0)
            {
                decimal earningsGrowthScore = NonlinearGrowthScore(data.Growth.EarningsGrowth, 0.05m, 0.30m);
                metricScores["EarningsGrowth"] = earningsGrowthScore;
            }
            
            // Růst FCF
            if (data.Growth.PredictedFCFGrowth != 0)
            {
                decimal fcfGrowthScore = NonlinearGrowthScore(data.Growth.PredictedFCFGrowth, 0.05m, 0.30m);
                metricScores["FCFGrowth"] = fcfGrowthScore;
            }
            
            // Vážený průměr růstových skóre
            decimal totalGrowthScore = 0;
            decimal appliedWeight = 0;
            
            foreach (var metric in metricScores.Keys)
            {
                if (metricWeights.ContainsKey(metric))
                {
                    totalGrowthScore += metricScores[metric] * metricWeights[metric];
                    appliedWeight += metricWeights[metric];
                }
            }
            
            if (appliedWeight > 0)
                return totalGrowthScore / appliedWeight;
            else
                return 5.0m;
        }
        
        /// <summary>
        /// Nelineární skórovací funkce pro růstové metriky
        /// </summary>
        private decimal NonlinearGrowthScore(decimal growthRate, decimal medianGrowth, decimal exceptionalGrowth)
        {
            // Negativní růst
            if (growthRate < 0)
            {
                // 0-3 pro negativní růst (0 pro -20% nebo horší, 3 pro 0%)
                return Math.Max(0, 3.0m + (growthRate / 0.2m) * 3.0m);
            }
            // 0% až medián (typicky 3-5%)
            else if (growthRate < medianGrowth)
            {
                // 3-5 pro růst 0% až medián
                return 3.0m + (growthRate / medianGrowth) * 2.0m;
            }
            // Medián až výjimečný růst (typicky 5-25%)
            else if (growthRate < exceptionalGrowth)
            {
                // 5-9 pro růst medián až výjimečný
                return 5.0m + ((growthRate - medianGrowth) / (exceptionalGrowth - medianGrowth)) * 4.0m;
            }
            // Výjimečný nebo vyšší
            else
            {
                // 9-10 pro růst nad výjimečnou hodnotu (plochý nad určitou úrovní)
                return 9.0m + Math.Min(1.0m, (growthRate - exceptionalGrowth) / 0.1m);
            }
        }
        
        /// <summary>
        /// Vypočítá skóre kvality podnikání
        /// </summary>
        private decimal CalculateQualityScore(FundamentalData data)
        {
            Dictionary<string, decimal> metricWeights = new Dictionary<string, decimal>
            {
                { "ROE", 0.4m },
                { "NetMargin", 0.4m },
                { "AssetTurnover", 0.2m }
            };
            
            Dictionary<string, decimal> metricScores = new Dictionary<string, decimal>();
            
            // ROE - rentabilita vlastního kapitálu
            if (data.Profitability.ROE != 0)
            {
                decimal roeScore = NonlinearProfitabilityScore(data.Profitability.ROE, 0.10m, 0.20m);
                metricScores["ROE"] = roeScore;
            }
            
            // Čistá marže
            if (data.Profitability.NetMargin != 0)
            {
                decimal netMarginScore = NonlinearProfitabilityScore(data.Profitability.NetMargin, 0.08m, 0.20m);
                metricScores["NetMargin"] = netMarginScore;
            }
            
            // Obrat aktiv (pokud je dostupný)
            if (data.Profitability.AssetTurnover > 0)
            {
                decimal turnoverScore = NonlinearEfficiencyScore(data.Profitability.AssetTurnover, 0.6m, 1.2m);
                metricScores["AssetTurnover"] = turnoverScore;
            }
            
            // Vážený průměr skóre kvality
            decimal totalQualityScore = 0;
            decimal appliedWeight = 0;
            
            foreach (var metric in metricScores.Keys)
            {
                if (metricWeights.ContainsKey(metric))
                {
                    totalQualityScore += metricScores[metric] * metricWeights[metric];
                    appliedWeight += metricWeights[metric];
                }
            }
            
            if (appliedWeight > 0)
                return totalQualityScore / appliedWeight;
            else
                return 5.0m;
        }
        
        /// <summary>
        /// Nelineární skórovací funkce pro metriky profitability
        /// </summary>
        private decimal NonlinearProfitabilityScore(decimal profitabilityValue, decimal medianValue, decimal exceptionalValue)
        {
            // Negativní hodnota
            if (profitabilityValue < 0)
            {
                // 0-2 pro negativní profitabilitu
                return Math.Max(0, 2.0m + (profitabilityValue / 0.1m) * 2.0m);
            }
            // 0 až medián
            else if (profitabilityValue < medianValue)
            {
                // 2-5 pro profitabilitu 0 až medián
                return 2.0m + (profitabilityValue / medianValue) * 3.0m;
            }
            // Medián až výjimečná hodnota
            else if (profitabilityValue < exceptionalValue)
            {
                // 5-9 pro profitabilitu medián až výjimečná
                return 5.0m + ((profitabilityValue - medianValue) / (exceptionalValue - medianValue)) * 4.0m;
            }
            // Výjimečná nebo vyšší
            else
            {
                // 9-10 pro profitabilitu nad výjimečnou hodnotu
                return 9.0m + Math.Min(1.0m, (profitabilityValue - exceptionalValue) / 0.05m);
            }
        }
        
        /// <summary>
        /// Nelineární skórovací funkce pro metriky efektivity
        /// </summary>
        private decimal NonlinearEfficiencyScore(decimal efficiencyValue, decimal medianValue, decimal exceptionalValue)
        {
            // Nízká hodnota
            if (efficiencyValue < medianValue / 2)
            {
                // 0-3 pro velmi nízkou efektivitu
                return Math.Max(0, 3.0m * (efficiencyValue / (medianValue / 2)));
            }
            // Nízká až medián
            else if (efficiencyValue < medianValue)
            {
                // 3-5 pro efektivitu nízkou až medián
                return 3.0m + ((efficiencyValue - (medianValue / 2)) / (medianValue / 2)) * 2.0m;
            }
            // Medián až výjimečná hodnota
            else if (efficiencyValue < exceptionalValue)
            {
                // 5-9 pro efektivitu medián až výjimečná
                return 5.0m + ((efficiencyValue - medianValue) / (exceptionalValue - medianValue)) * 4.0m;
            }
            // Výjimečná nebo vyšší
            else
            {
                // 9-10 pro efektivitu nad výjimečnou hodnotu, s limitem
                return 9.0m + Math.Min(1.0m, (efficiencyValue - exceptionalValue) / 0.2m);
            }
        }
        
        /// <summary>
        /// Vypočítá skóre finančního zdraví
        /// </summary>
        private decimal CalculateFinancialHealthScore(FundamentalData data)
        {
            Dictionary<string, decimal> metricWeights = new Dictionary<string, decimal>
            {
                { "DebtToEquity", 0.30m },
                { "CurrentRatio", 0.25m },
                { "InterestCoverage", 0.25m },
                { "QuickRatio", 0.20m }
            };
            
            Dictionary<string, decimal> metricScores = new Dictionary<string, decimal>();
            
            // Debt/Equity poměr (nižší = lepší)
            if (data.Stability.DebtToEquity >= 0)
            {
                decimal debtToEquityScore;
                
                if (data.Stability.DebtToEquity < 0.1m)
                    debtToEquityScore = 10.0m;
                else if (data.Stability.DebtToEquity < 0.5m)
                    debtToEquityScore = 9.0m - ((data.Stability.DebtToEquity - 0.1m) / 0.4m) * 2.0m;
                else if (data.Stability.DebtToEquity < 1.0m)
                    debtToEquityScore = 7.0m - ((data.Stability.DebtToEquity - 0.5m) / 0.5m) * 2.0m;
                else if (data.Stability.DebtToEquity < 1.5m)
                    debtToEquityScore = 5.0m - ((data.Stability.DebtToEquity - 1.0m) / 0.5m) * 2.0m;
                else if (data.Stability.DebtToEquity < 2.5m)
                    debtToEquityScore = 3.0m - ((data.Stability.DebtToEquity - 1.5m) / 1.0m) * 2.0m;
                else
                    debtToEquityScore = Math.Max(0, 1.0m - ((data.Stability.DebtToEquity - 2.5m) / 2.5m));
                
                metricScores["DebtToEquity"] = debtToEquityScore;
            }
            
            // Current Ratio (likvidita) - vyšší = lepší, ale příliš vysoké může být neefektivní
            if (data.Stability.CurrentRatio > 0)
            {
                decimal currentRatioScore;
                
                if (data.Stability.CurrentRatio < 0.5m)
                    currentRatioScore = Math.Max(0, data.Stability.CurrentRatio / 0.5m * 2.0m);
                else if (data.Stability.CurrentRatio < 1.0m)
                    currentRatioScore = 2.0m + ((data.Stability.CurrentRatio - 0.5m) / 0.5m) * 3.0m;
                else if (data.Stability.CurrentRatio < 1.5m)
                    currentRatioScore = 5.0m + ((data.Stability.CurrentRatio - 1.0m) / 0.5m) * 2.0m;
                else if (data.Stability.CurrentRatio < 2.0m)
                    currentRatioScore = 7.0m + ((data.Stability.CurrentRatio - 1.5m) / 0.5m) * 2.0m;
                else if (data.Stability.CurrentRatio < 3.0m)
                    currentRatioScore = 9.0m + ((data.Stability.CurrentRatio - 2.0m) / 1.0m);
                else
                    currentRatioScore = 10.0m; // Více než 3.0 je už dostatečně likvidní
                
                metricScores["CurrentRatio"] = currentRatioScore;
            }
            
            // Interest Coverage (úrokové krytí) - vyšší = lepší
            if (data.Stability.InterestCoverage > 0)
            {
                decimal interestCoverageScore;
                
                if (data.Stability.InterestCoverage < 1.0m)
                    interestCoverageScore = Math.Max(0, data.Stability.InterestCoverage * 2.0m);
                else if (data.Stability.InterestCoverage < 2.0m)
                    interestCoverageScore = 2.0m + ((data.Stability.InterestCoverage - 1.0m) / 1.0m) * 2.0m;
                else if (data.Stability.InterestCoverage < 4.0m)
                    interestCoverageScore = 4.0m + ((data.Stability.InterestCoverage - 2.0m) / 2.0m) * 2.0m;
                else if (data.Stability.InterestCoverage < 8.0m)
                    interestCoverageScore = 6.0m + ((data.Stability.InterestCoverage - 4.0m) / 4.0m) * 2.0m;
                else if (data.Stability.InterestCoverage < 15.0m)
                    interestCoverageScore = 8.0m + ((data.Stability.InterestCoverage - 8.0m) / 7.0m) * 2.0m;
                else
                    interestCoverageScore = 10.0m; // Více než 15.0 je již vynikající
                
                metricScores["InterestCoverage"] = interestCoverageScore;
            }
            
            // Quick Ratio (pokud je dostupné)
            if (data.Stability.QuickRatio > 0)
            {
                decimal quickRatioScore;
                
                if (data.Stability.QuickRatio < 0.5m)
                    quickRatioScore = Math.Max(0, data.Stability.QuickRatio / 0.5m * 3.0m);
                else if (data.Stability.QuickRatio < 1.0m)
                    quickRatioScore = 3.0m + ((data.Stability.QuickRatio - 0.5m) / 0.5m) * 3.0m;
                else if (data.Stability.QuickRatio < 1.5m)
                    quickRatioScore = 6.0m + ((data.Stability.QuickRatio - 1.0m) / 0.5m) * 2.0m;
                else if (data.Stability.QuickRatio < 2.0m)
                    quickRatioScore = 8.0m + ((data.Stability.QuickRatio - 1.5m) / 0.5m) * 2.0m;
                else
                    quickRatioScore = 10.0m; // Více než 2.0 je už velmi dobré
                
                metricScores["QuickRatio"] = quickRatioScore;
            }
            
            // Vážený průměr skóre finančního zdraví
            decimal totalFinancialHealthScore = 0;
            decimal appliedWeight = 0;
            
            foreach (var metric in metricScores.Keys)
            {
                if (metricWeights.ContainsKey(metric))
                {
                    totalFinancialHealthScore += metricScores[metric] * metricWeights[metric];
                    appliedWeight += metricWeights[metric];
                }
            }
            
            if (appliedWeight > 0)
                return totalFinancialHealthScore / appliedWeight;
            else
                return 5.0m;
        }
        
        /// <summary>
        /// Vypočítá rizikové skóre (vyšší = nižší riziko)
        /// </summary>
        private decimal CalculateRiskScore(FundamentalData data)
        {
            Dictionary<string, decimal> metricWeights = new Dictionary<string, decimal>
            {
                { "Beta", 0.40m },
                { "Volatility", 0.40m },
                { "EarningsStability", 0.20m }
            };
            
            Dictionary<string, decimal> metricScores = new Dictionary<string, decimal>();
            
            // Beta (nižší = nižší volatilita = nižší riziko = vyšší skóre)
            if (data.MarketRisk.Beta > 0)
            {
                decimal betaScore;
                
                if (data.MarketRisk.Beta < 0.5m)
                    betaScore = 10.0m;
                else if (data.MarketRisk.Beta < 0.75m)
                    betaScore = 9.0m - ((data.MarketRisk.Beta - 0.5m) / 0.25m);
                else if (data.MarketRisk.Beta < 1.0m)
                    betaScore = 8.0m - ((data.MarketRisk.Beta - 0.75m) / 0.25m);
                else if (data.MarketRisk.Beta < 1.25m)
                    betaScore = 6.0m - ((data.MarketRisk.Beta - 1.0m) / 0.25m) * 2.0m;
                else if (data.MarketRisk.Beta < 1.5m)
                    betaScore = 4.0m - ((data.MarketRisk.Beta - 1.25m) / 0.25m) * 2.0m;
                else if (data.MarketRisk.Beta < 2.0m)
                    betaScore = 2.0m - ((data.MarketRisk.Beta - 1.5m) / 0.5m) * 2.0m;
                else
                    betaScore = Math.Max(0, 1.0m - ((data.MarketRisk.Beta - 2.0m) / 1.0m));
                
                metricScores["Beta"] = betaScore;
            }
            
            // Volatilita (nižší = nižší riziko = vyšší skóre)
            if (data.MarketRisk.Volatility > 0)
            {
                decimal volatilityScore;
                
                if (data.MarketRisk.Volatility < 0.15m)
                    volatilityScore = 10.0m - ((data.MarketRisk.Volatility) / 0.15m);
                else if (data.MarketRisk.Volatility < 0.25m)
                    volatilityScore = 9.0m - ((data.MarketRisk.Volatility - 0.15m) / 0.1m) * 2.0m;
                else if (data.MarketRisk.Volatility < 0.35m)
                    volatilityScore = 7.0m - ((data.MarketRisk.Volatility - 0.25m) / 0.1m) * 2.0m;
                else if (data.MarketRisk.Volatility < 0.5m)
                    volatilityScore = 5.0m - ((data.MarketRisk.Volatility - 0.35m) / 0.15m) * 3.0m;
                else if (data.MarketRisk.Volatility < 0.7m)
                    volatilityScore = 2.0m - ((data.MarketRisk.Volatility - 0.5m) / 0.2m) * 2.0m;
                else
                    volatilityScore = Math.Max(0, 1.0m - ((data.MarketRisk.Volatility - 0.7m) / 0.3m));
                
                metricScores["Volatility"] = volatilityScore;
            }
            
            // Stabilita zisků (pokud je dostupná)
            if (data.Stability.EarningsStability > 0)
            {
                decimal earningsStabilityScore;
                
                // Vyšší hodnota = vyšší stabilita = nižší riziko = vyšší skóre
                if (data.Stability.EarningsStability > 0.9m)
                    earningsStabilityScore = 10.0m;
                else if (data.Stability.EarningsStability > 0.8m)
                    earningsStabilityScore = 9.0m - ((0.9m - data.Stability.EarningsStability) / 0.1m);
                else if (data.Stability.EarningsStability > 0.6m)
                    earningsStabilityScore = 8.0m - ((0.8m - data.Stability.EarningsStability) / 0.2m) * 3.0m;
                else if (data.Stability.EarningsStability > 0.4m)
                    earningsStabilityScore = 5.0m - ((0.6m - data.Stability.EarningsStability) / 0.2m) * 3.0m;
                else if (data.Stability.EarningsStability > 0.2m)
                    earningsStabilityScore = 2.0m - ((0.4m - data.Stability.EarningsStability) / 0.2m) * 2.0m;
                else
                    earningsStabilityScore = Math.Max(0, 1.0m - ((0.2m - data.Stability.EarningsStability) / 0.2m));
                
                metricScores["EarningsStability"] = earningsStabilityScore;
            }
            
            // Vážený průměr rizikového skóre
            decimal totalRiskScore = 0;
            decimal appliedWeight = 0;
            
            foreach (var metric in metricScores.Keys)
            {
                if (metricWeights.ContainsKey(metric))
                {
                    totalRiskScore += metricScores[metric] * metricWeights[metric];
                    appliedWeight += metricWeights[metric];
                }
            }
            
            if (appliedWeight > 0)
                return totalRiskScore / appliedWeight;
            else
                return 5.0m;
        }
        
        /// <summary>
        /// Vypočítá skóre sentimentu
        /// </summary>
        private decimal CalculateSentimentScore(FundamentalData data)
        {
            Dictionary<string, decimal> metricWeights = new Dictionary<string, decimal>
            {
                { "AnalystConsensus", 0.60m },
                { "InstitutionalBuying", 0.20m },
                { "InsiderBuying", 0.20m }
            };
            
            Dictionary<string, decimal> metricScores = new Dictionary<string, decimal>();
            
            // Analýzy analytiků
            if (data.Sentiment.AnalystConsensus > 0)
            {
                // Předpokládáme, že AnalystConsensus je na škále 1-5, kde 5 je nejlepší (silný nákup)
                decimal analystScore = (data.Sentiment.AnalystConsensus / 5.0m) * 10.0m;
                metricScores["AnalystConsensus"] = analystScore;
            }
            
            // Institucionální nákupy (pokud jsou dostupné)
            if (data.Sentiment.InstitutionalOwnership > 0)
            {
                // Nákupy institucionálních investorů, škála 0-1 kde vyšší = více nákupů
                decimal institutionalScore = data.Sentiment.InstitutionalOwnership * 10.0m;
                metricScores["InstitutionalBuying"] = institutionalScore;
            }
            
            // Insider nákupy (pokud jsou dostupné)
            if (data.Sentiment.InsiderBuying > 0)
            {
                // Nákupy insiderů, škála 0-1 kde vyšší = více nákupů
                decimal insiderScore = data.Sentiment.InsiderBuying * 10.0m;
                metricScores["InsiderBuying"] = insiderScore;
            }
            
            // Vážený průměr skóre sentimentu
            decimal totalSentimentScore = 0;
            decimal appliedWeight = 0;
            
            foreach (var metric in metricScores.Keys)
            {
                if (metricWeights.ContainsKey(metric))
                {
                    totalSentimentScore += metricScores[metric] * metricWeights[metric];
                    appliedWeight += metricWeights[metric];
                }
            }
            
            if (appliedWeight > 0)
                return totalSentimentScore / appliedWeight;
            else
                return 5.0m;
        }
        
        /// <summary>
        /// Vypočítá skóre likvidity akcií
        /// </summary>
        private decimal CalculateLiquidityScore(FundamentalData data)
        {
            // Tento model by v reálné implementaci používal:
            // - Průměrný denní objem obchodů
            // - Bid-ask spread
            // - Free float (podíl akcií volně obchodovaných na trhu)
            
            // Zjednodušená implementace:
            // Předpokládáme střední likviditu, pokud nemáme data
            return 5.0m;
        }
        
        /// <summary>
        /// Vypočítá ESG skóre
        /// </summary>
        private decimal CalculateESGScore(FundamentalData data)
        {
            // Tento model by v reálné implementaci používal:
            // - Environmentální skóre (emise, obnovitelná energie, odpadové hospodářství)
            // - Sociální skóre (diverzita, lidská práva, pracovní podmínky)
            // - Governance skóre (struktura představenstva, executive kompenzace, transparentnost)
            
            // Zjednodušená implementace:
            // Předpokládáme průměrné ESG skóre, pokud nemáme data
            return 5.0m;
        }
        
        /// <summary>
        /// Nelineární převod skóre na cenové rozpětí/potenciál
        /// </summary>
        private decimal CalculateNonlinearPriceAdjustment(decimal score)
        {
            // Nelineární funkce pro převod skóre na cenový potenciál:
            
            if (score < 2.5m)
            {
                // Silně nadhodnocené (0-2.5): -50% až -25% potenciál
                // Exponenciální funkce pro výraznější snížení u velmi nízkých skóre
                return -0.5m + (0.25m * (score / 2.5m) * (score / 2.5m));
            }
            else if (score < 5.0m)
            {
                // Mírně nadhodnocené (2.5-5.0): -25% až 0% potenciál
                // Lineární přechod
                return -0.25m + (0.25m * (score - 2.5m) / 2.5m);
            }
            else if (score < 7.5m)
            {
                // Mírně podhodnocené (5.0-7.5): 0% až 50% potenciál
                // Lineární přechod, ale strmější než u nadhodnocených
                return 0m + (0.5m * (score - 5.0m) / 2.5m);
            }
            else
            {
                // Silně podhodnocené (7.5-10.0): 50% až 100% potenciál
                // Exponenciální nárůst pro vyšší potenciál u velmi vysokých skóre
                decimal base50 = 0.5m;
                decimal additional = (score - 7.5m) / 2.5m; // 0-1 range
                decimal exponential = additional * additional * 0.5m; // Kvadratický nárůst, max 0.5
                return base50 + exponential;
            }
        }
    }
}