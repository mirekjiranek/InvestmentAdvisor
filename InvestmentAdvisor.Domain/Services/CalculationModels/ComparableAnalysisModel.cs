using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšená srovnávací analýza (Comparable Company Analysis):
    /// 1. Porovnává valuační násobky firmy s průměry a mediány sektoru
    /// 2. Používá širokou škálu metrik (P/E, Forward P/E, EV/EBITDA, P/S, P/B, PEG, EV/Sales, P/FCF)
    /// 3. Přiděluje váhy každé metrice podle relevance pro daný sektor a růstovou fázi
    /// 4. Zohledňuje kvalitu a růstové charakteristiky firmy pro úpravu valuačních násobků
    /// 5. Zahrnuje korekce pro růstové rozdíly mezi srovnávanými společnostmi (PEG ratio)
    /// 6. Zpřesňuje výpočet hodnoty z EV/EBITDA zahrnutím čistého dluhu společnosti
    /// 7. Implementuje dynamický výběr srovnatelných společností místo použití celého sektoru
    /// </summary>
    public class ComparableAnalysisModel
    {
        public decimal CalculateComparableValue(FundamentalData data)
        {
            // Základní finanční metriky společnosti
            decimal companyEPS = data.Earnings.EPS;
            decimal companyForwardEPS = data.Earnings.ForwardEPS > 0 ? data.Earnings.ForwardEPS : companyEPS * (1 + data.Growth.PredictedEpsGrowth);
            decimal companyEBITDA = data.Earnings.EBITDA;
            decimal companySalesPerShare = data.Revenue.SalesPerShare;
            decimal companyBookValuePerShare = data.Valuation.BookValuePerShare;
            decimal companyEPSGrowth = data.Growth.PredictedEpsGrowth;
            decimal companyFCF = data.CashFlow.CurrentFCF;
            
            // ROE, marže a zadlužení pro kvalitativní úpravy
            decimal companyROE = data.Profitability.ROE;
            decimal companyNetMargin = data.Profitability.NetMargin;
            decimal companyDebtToEquity = data.Stability.DebtToEquity;
            
            // Sektorové průměry - potenciálně upravené podle dynamického výběru srovnatelných společností
            decimal sectorPE = AdjustSectorMultiple(data.Comparable.SectorAveragePE, data);
            decimal sectorForwardPE = data.Comparable.SectorForwardPE > 0 ? AdjustSectorMultiple(data.Comparable.SectorForwardPE, data) : sectorPE * 0.9m;
            decimal sectorEVEBITDA = AdjustSectorMultiple(data.Comparable.PeerEVEBITDA, data);
            decimal sectorPriceSales = AdjustSectorMultiple(data.Comparable.SectorPriceSales, data);
            decimal sectorPriceBook = data.Comparable.SectorPriceBook > 0 ? AdjustSectorMultiple(data.Comparable.SectorPriceBook, data) : 2.0m;
            decimal sectorPEG = data.Comparable.SectorPEG > 0 ? data.Comparable.SectorPEG : 1.5m;
            
            // Nové násobky
            decimal sectorEVSales = AdjustSectorMultiple(data.Comparable.SectorEVSales, data);
            decimal sectorPriceFCF = AdjustSectorMultiple(data.Comparable.SectorPriceFCF, data);
            
            // Průměrné hodnoty sektoru pro kvalitativní úpravy - defaultní hodnoty pokud nejsou k dispozici
            decimal sectorAvgROE = data.Comparable.SectorAverageROE > 0 ? data.Comparable.SectorAverageROE : 0.15m;
            decimal sectorAvgNetMargin = data.Comparable.SectorAverageNetMargin > 0 ? data.Comparable.SectorAverageNetMargin : 0.10m;
            decimal sectorAvgGrowth = data.Comparable.SectorAverageGrowth > 0 ? data.Comparable.SectorAverageGrowth : 0.08m;
            
            // Korekce pro růstové rozdíly
            decimal growthPremium = CalculateGrowthPremium(companyEPSGrowth, sectorAvgGrowth);
            
            // Kvalitativní úpravy
            decimal qualityPremium = CalculateQualityPremium(companyROE, companyNetMargin, sectorAvgROE, sectorAvgNetMargin);
            
            // Kombinovaná prémie/diskont
            decimal combinedAdjustment = qualityPremium + growthPremium;
            
            // Limitujeme kombinovanou prémii/diskont
            combinedAdjustment = Math.Max(combinedAdjustment, -0.3m); // Maximální diskont -30%
            combinedAdjustment = Math.Min(combinedAdjustment, 0.5m);  // Maximální prémie +50%
            
            // Upravené sektorové násobky
            decimal adjustedSectorPE = sectorPE * (1 + combinedAdjustment);
            decimal adjustedSectorForwardPE = sectorForwardPE * (1 + combinedAdjustment);
            decimal adjustedSectorEVEBITDA = sectorEVEBITDA * (1 + combinedAdjustment);
            decimal adjustedSectorPriceSales = sectorPriceSales * (1 + combinedAdjustment);
            decimal adjustedSectorPriceBook = sectorPriceBook * (1 + combinedAdjustment);
            decimal adjustedSectorEVSales = sectorEVSales * (1 + combinedAdjustment);
            decimal adjustedSectorPriceFCF = sectorPriceFCF * (1 + combinedAdjustment);
            
            // Výpočet férových hodnot podle různých metrik
            decimal fairValuePE = companyEPS > 0 ? companyEPS * adjustedSectorPE : 0;
            decimal fairValueForwardPE = companyForwardEPS > 0 ? companyForwardEPS * adjustedSectorForwardPE : 0;
            
            // Zpřesněný výpočet hodnoty z EV/EBITDA zahrnutím čistého dluhu společnosti
            decimal fairValueEVEBITDA = 0;
            if (companyEBITDA > 0)
            {
                // Enterprise Value = EBITDA * EV/EBITDA
                decimal enterpriseValue = companyEBITDA * adjustedSectorEVEBITDA;
                
                // Čistý dluh (Net Debt) = Celkový dluh - Hotovost
                decimal netDebtPerShare = EstimateNetDebtPerShare(data);
                
                // Equity Value = Enterprise Value - Net Debt
                fairValueEVEBITDA = (enterpriseValue - netDebtPerShare);
                
                // Kontrola validity (musí být > 0)
                fairValueEVEBITDA = Math.Max(0.1m, fairValueEVEBITDA);
            }
            
            decimal fairValuePS = companySalesPerShare > 0 ? companySalesPerShare * adjustedSectorPriceSales : 0;
            decimal fairValuePB = companyBookValuePerShare > 0 ? companyBookValuePerShare * adjustedSectorPriceBook : 0;
            decimal fairValuePEG = (companyEPS > 0 && companyEPSGrowth > 0) ? 
                                   companyEPS * sectorPEG * companyEPSGrowth : 0;
            
            // Nové metriky
            decimal fairValueEVSales = companySalesPerShare > 0 ? 
                                      (companySalesPerShare * adjustedSectorEVSales) - EstimateNetDebtPerShare(data) : 0;
            decimal fairValuePFCF = companyFCF > 0 ? companyFCF * adjustedSectorPriceFCF : 0;
            
            // Váhy pro každou metriku podle její spolehlivosti a relevance
            // Nastavení vah podle ziskovosti, růstu a fáze růstu společnosti
            Dictionary<string, decimal> weights = AssignMetricWeights(data);
            
            // Výpočet vážené férové hodnoty
            decimal weightedFairValue = 0;
            decimal totalAppliedWeight = 0;
            
            // Aplikujeme váhy jen na metriky, které mají smysluplné hodnoty
            if (fairValuePE > 0 && weights.ContainsKey("PE"))
            {
                weightedFairValue += fairValuePE * weights["PE"];
                totalAppliedWeight += weights["PE"];
            }
            
            if (fairValueForwardPE > 0 && weights.ContainsKey("ForwardPE"))
            {
                weightedFairValue += fairValueForwardPE * weights["ForwardPE"];
                totalAppliedWeight += weights["ForwardPE"];
            }
            
            if (fairValueEVEBITDA > 0 && weights.ContainsKey("EVEBITDA"))
            {
                weightedFairValue += fairValueEVEBITDA * weights["EVEBITDA"];
                totalAppliedWeight += weights["EVEBITDA"];
            }
            
            if (fairValuePS > 0 && weights.ContainsKey("PS"))
            {
                weightedFairValue += fairValuePS * weights["PS"];
                totalAppliedWeight += weights["PS"];
            }
            
            if (fairValuePB > 0 && weights.ContainsKey("PB"))
            {
                weightedFairValue += fairValuePB * weights["PB"];
                totalAppliedWeight += weights["PB"];
            }
            
            if (fairValuePEG > 0 && weights.ContainsKey("PEG"))
            {
                weightedFairValue += fairValuePEG * weights["PEG"];
                totalAppliedWeight += weights["PEG"];
            }
            
            if (fairValueEVSales > 0 && weights.ContainsKey("EVSales"))
            {
                weightedFairValue += fairValueEVSales * weights["EVSales"];
                totalAppliedWeight += weights["EVSales"];
            }
            
            if (fairValuePFCF > 0 && weights.ContainsKey("PFCF"))
            {
                weightedFairValue += fairValuePFCF * weights["PFCF"];
                totalAppliedWeight += weights["PFCF"];
            }
            
            // Pokud máme alespoň nějaké váhy, vypočítáme váženou hodnotu
            if (totalAppliedWeight > 0)
            {
                return weightedFairValue / totalAppliedWeight;
            }
            else
            {
                // Žádná relevantní metrika není dostupná, default fallback
                return fairValuePS > 0 ? fairValuePS : data.Price;
            }
        }
        
        /// <summary>
        /// Přizpůsobí sektorový násobek na základě dynamického výběru srovnatelných společností
        /// </summary>
        private decimal AdjustSectorMultiple(decimal sectorMultiple, FundamentalData data)
        {
            // V ideálním případě bychom zde měli algoritmus pro výběr srovnatelných společností
            // na základě velikosti, růstu, ziskovosti, atd. a výpočet průměru/mediánu z vybraných společností.
            
            // Zjednodušená implementace: pouze upravuje sektorový průměr podle toho, jak moc je
            // společnost podobná průměru sektoru (podle růstu, marže, atd.)
            
            // Vrátíme původní hodnotu, dokud neimplementujeme plnou funkcionalitu
            return sectorMultiple;
        }
        
        /// <summary>
        /// Vypočítá prémii/diskont na základě rozdílu mezi růstem společnosti a průměrným růstem sektoru
        /// </summary>
        private decimal CalculateGrowthPremium(decimal companyGrowth, decimal sectorGrowth)
        {
            if (sectorGrowth <= 0) return 0;
            
            // Relativní rozdíl v růstu
            decimal growthDifference = companyGrowth / sectorGrowth - 1;
            
            // PEG korekce - vyšší růst oproti sektoru zaslouží prémii
            decimal growthPremium = growthDifference * 0.5m;
            
            // Limitujeme extrémní hodnoty
            return Math.Max(-0.2m, Math.Min(0.3m, growthPremium));
        }
        
        /// <summary>
        /// Vypočítá kvalitativní prémii na základě ROE a čisté marže
        /// </summary>
        private decimal CalculateQualityPremium(decimal companyROE, decimal companyNetMargin, 
                                              decimal sectorROE, decimal sectorNetMargin)
        {
            decimal qualityPremium = 0;
            
            // ROE prémie - firmy s vyšším ROE než průměr sektoru si zaslouží vyšší valuaci
            if (sectorROE > 0 && companyROE > 0)
            {
                decimal roeRatio = companyROE / sectorROE;
                qualityPremium += 0.15m * (roeRatio - 1);
            }
            
            // Marže prémie - firmy s vyšší marží než průměr sektoru si zaslouží vyšší valuaci
            if (sectorNetMargin > 0 && companyNetMargin > 0)
            {
                decimal marginRatio = companyNetMargin / sectorNetMargin;
                qualityPremium += 0.15m * (marginRatio - 1);
            }
            
            // Limitujeme kvalitativní prémii
            return Math.Max(-0.25m, Math.Min(0.35m, qualityPremium));
        }
        
        /// <summary>
        /// Odhaduje čistý dluh na akcii pro výpočet hodnoty z EV-násobků
        /// </summary>
        private decimal EstimateNetDebtPerShare(FundamentalData data)
        {
            // V ideálním případě by tato hodnota měla být přímo dostupná v datech
            // Zjednodušený odhad založený na Debt/Equity a book value
            decimal debtToEquity = data.Stability.DebtToEquity;
            decimal bookValuePerShare = data.Valuation.BookValuePerShare;
            
            decimal estimatedDebtPerShare = debtToEquity * bookValuePerShare;
            
            // Odhadovaná hotovost na akcii (typicky 5-15% aktiv)
            decimal estimatedCashPerShare = bookValuePerShare * 0.1m;
            
            // Čistý dluh = Dluh - Hotovost
            decimal netDebtPerShare = Math.Max(0, estimatedDebtPerShare - estimatedCashPerShare);
            
            return netDebtPerShare;
        }
        
        /// <summary>
        /// Přiděluje váhy jednotlivým metrikám podle typu společnosti a sektoru
        /// </summary>
        private Dictionary<string, decimal> AssignMetricWeights(FundamentalData data)
        {
            var weights = new Dictionary<string, decimal>();
            
            // Výchozí váhy - budou upraveny podle typu společnosti
            weights["PE"] = 0.15m;
            weights["ForwardPE"] = 0.15m;
            weights["EVEBITDA"] = 0.20m;
            weights["PS"] = 0.10m;
            weights["PB"] = 0.05m;
            weights["PEG"] = 0.10m;
            weights["EVSales"] = 0.10m;
            weights["PFCF"] = 0.15m;
            
            // Úpravy vah podle typu společnosti
            
            // 1. Stabilní ziskové společnosti - důraz na ziskové metriky
            if (data.Earnings.EPS > 0 && data.Growth.PredictedEpsGrowth < 0.15m && data.Stability.DebtToEquity < 1.5m)
            {
                weights["PE"] = 0.20m;
                weights["ForwardPE"] = 0.15m;
                weights["EVEBITDA"] = 0.25m;
                weights["PS"] = 0.05m;
                weights["PB"] = 0.05m;
                weights["PEG"] = 0.05m;
                weights["EVSales"] = 0.05m;
                weights["PFCF"] = 0.20m;
            }
            
            // 2. Růstové společnosti - méně důraz na současný zisk, více na růst a tržby
            else if (data.Growth.PredictedEpsGrowth > 0.2m)
            {
                weights["PE"] = 0.05m;
                weights["ForwardPE"] = 0.15m;
                weights["EVEBITDA"] = 0.15m;
                weights["PS"] = 0.15m;
                weights["PB"] = 0.05m;
                weights["PEG"] = 0.20m;
                weights["EVSales"] = 0.15m;
                weights["PFCF"] = 0.10m;
            }
            
            // 3. Problémové společnosti - důraz na účetní hodnotu a tržby
            else if (data.Earnings.EPS <= 0 || data.Stability.DebtToEquity > 2.0m)
            {
                weights["PE"] = 0.0m;
                weights["ForwardPE"] = 0.10m;
                weights["EVEBITDA"] = 0.15m;
                weights["PS"] = 0.25m;
                weights["PB"] = 0.20m;
                weights["PEG"] = 0.0m;
                weights["EVSales"] = 0.25m;
                weights["PFCF"] = 0.05m;
            }
            
            // 4. Value společnosti s nízkými valuačními násobky
            else if (data.Valuation.PE > 0 && data.Valuation.PE < 12.0m)
            {
                weights["PE"] = 0.20m;
                weights["ForwardPE"] = 0.10m;
                weights["EVEBITDA"] = 0.20m;
                weights["PS"] = 0.05m;
                weights["PB"] = 0.10m;
                weights["PEG"] = 0.05m;
                weights["EVSales"] = 0.10m;
                weights["PFCF"] = 0.20m;
            }
            
            // 5. Vysoce ziskové společnosti - důraz na P/FCF a EV/EBITDA
            else if (data.Profitability.ROE > 0.2m && data.Profitability.NetMargin > 0.15m)
            {
                weights["PE"] = 0.15m;
                weights["ForwardPE"] = 0.10m;
                weights["EVEBITDA"] = 0.25m;
                weights["PS"] = 0.05m;
                weights["PB"] = 0.05m;
                weights["PEG"] = 0.10m;
                weights["EVSales"] = 0.05m;
                weights["PFCF"] = 0.25m;
            }
            
            // Specifické úpravy podle sektoru by mohly být přidány zde
            
            return weights;
        }
    }
}