using Domain.Entities;
using System;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšená srovnávací analýza:
    /// 1. Porovnává valuační násobky firmy s průměry a mediány sektoru
    /// 2. Používá více metrik (P/E, Forward P/E, EV/EBITDA, P/S, P/B, PEG)
    /// 3. Přiděluje váhy každé metrice podle relevance pro daný sektor a růstovou fázi
    /// 4. Zohledňuje kvalitu a růstové charakteristiky firmy pro úpravu valuačních násobků
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
            
            // ROE a marže pro kvalitativní úpravy
            decimal companyROE = data.Profitability.ROE;
            decimal companyNetMargin = data.Profitability.NetMargin;
            
            // Sektorové průměry
            decimal sectorPE = data.Comparable.SectorAveragePE;
            decimal sectorForwardPE = data.Comparable.SectorForwardPE > 0 ? data.Comparable.SectorForwardPE : sectorPE * 0.9m;
            decimal sectorEVEBITDA = data.Comparable.PeerEVEBITDA;
            decimal sectorPriceSales = data.Comparable.SectorPriceSales;
            decimal sectorPriceBook = data.Comparable.SectorPriceBook > 0 ? data.Comparable.SectorPriceBook : 2.0m;
            decimal sectorPEG = data.Comparable.SectorPEG > 0 ? data.Comparable.SectorPEG : 1.5m;
            
            // Průměrné hodnoty sektoru pro kvalitativní úpravy - defaultní hodnoty pokud nejsou k dispozici
            decimal sectorAvgROE = data.Comparable.SectorAverageROE > 0 ? data.Comparable.SectorAverageROE : 0.15m;
            decimal sectorAvgNetMargin = data.Comparable.SectorAverageNetMargin > 0 ? data.Comparable.SectorAverageNetMargin : 0.10m;
            
            // Kvalitativní úpravy
            decimal qualityPremium = 0;
            
            // ROE prémie - firmy s vyšším ROE než průměr sektoru si zaslouží vyšší valuaci
            if (companyROE > sectorAvgROE)
            {
                qualityPremium += 0.1m * (companyROE / sectorAvgROE - 1);
            }
            
            // Marže prémie - firmy s vyšší marží než průměr sektoru si zaslouží vyšší valuaci
            if (companyNetMargin > sectorAvgNetMargin)
            {
                qualityPremium += 0.1m * (companyNetMargin / sectorAvgNetMargin - 1);
            }
            
            // Limitujeme kvalitativní prémii
            qualityPremium = Math.Min(qualityPremium, 0.3m);
            
            // Upravené sektorové násobky
            decimal adjustedSectorPE = sectorPE * (1 + qualityPremium);
            decimal adjustedSectorForwardPE = sectorForwardPE * (1 + qualityPremium);
            decimal adjustedSectorEVEBITDA = sectorEVEBITDA * (1 + qualityPremium);
            decimal adjustedSectorPriceSales = sectorPriceSales * (1 + qualityPremium);
            decimal adjustedSectorPriceBook = sectorPriceBook * (1 + qualityPremium);
            
            // Výpočet férových hodnot podle různých metrik
            decimal fairValuePE = companyEPS > 0 ? companyEPS * adjustedSectorPE : 0;
            decimal fairValueForwardPE = companyForwardEPS > 0 ? companyForwardEPS * adjustedSectorForwardPE : 0;
            decimal fairValueEVEBITDA = companyEBITDA > 0 ? companyEBITDA * adjustedSectorEVEBITDA : 0;
            decimal fairValuePS = companySalesPerShare > 0 ? companySalesPerShare * adjustedSectorPriceSales : 0;
            decimal fairValuePB = companyBookValuePerShare > 0 ? companyBookValuePerShare * adjustedSectorPriceBook : 0;
            decimal fairValuePEG = (companyEPS > 0 && companyEPSGrowth > 0) ? 
                                     companyEPS * sectorPEG * companyEPSGrowth : 0;
            
            // Váhy pro každou metriku podle její spolehlivosti a relevance
            // Nastavení vah podle ziskovosti a fáze růstu společnosti
            decimal weightPE = companyEPS > 0 ? 0.2m : 0;
            decimal weightForwardPE = companyForwardEPS > 0 ? 0.2m : 0;
            decimal weightEVEBITDA = companyEBITDA > 0 ? 0.25m : 0;
            decimal weightPS = 0.15m;
            decimal weightPB = 0.1m;
            decimal weightPEG = (companyEPS > 0 && companyEPSGrowth > 0) ? 0.1m : 0;
            
            // Normalizace vah, aby součet byl 1.0
            decimal totalWeight = weightPE + weightForwardPE + weightEVEBITDA + weightPS + weightPB + weightPEG;
            
            if (totalWeight > 0)
            {
                weightPE /= totalWeight;
                weightForwardPE /= totalWeight;
                weightEVEBITDA /= totalWeight;
                weightPS /= totalWeight;
                weightPB /= totalWeight;
                weightPEG /= totalWeight;
                
                // Výpočet vážené férové hodnoty
                decimal weightedFairValue = 
                    (fairValuePE * weightPE) + 
                    (fairValueForwardPE * weightForwardPE) + 
                    (fairValueEVEBITDA * weightEVEBITDA) + 
                    (fairValuePS * weightPS) + 
                    (fairValuePB * weightPB) + 
                    (fairValuePEG * weightPEG);
                
                return weightedFairValue;
            }
            else
            {
                // Žádná relevantní metrika není dostupná, default fallback
                return fairValuePS > 0 ? fairValuePS : 0;
            }
        }
    }
}