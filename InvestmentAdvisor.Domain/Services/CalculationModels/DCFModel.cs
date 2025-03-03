using Domain.Entities;
using System;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšený DCF model:
    /// 1. Používá předpovězené volné cash flow (FCF) s vícestupňovým růstovým modelem.
    /// 2. Dynamicky vypočítává WACC na základě kapitálové struktury, CAPM a ceny cizího kapitálu.
    /// 3. Přizpůsobuje růstové míry podle sektoru, velikosti společnosti a fáze životního cyklu.
    /// 4. Aplikuje faktor důvěry pro zohlednění nejistoty předpovědí.
    /// 5. Po horizontu projekce počítá terminální hodnotu (Gordon Growth Model).
    /// 6. Výsledek je současná hodnota všech budoucích toků.
    /// </summary>
    public class DCFModel
    {
        public decimal CalculateDCFValue(FundamentalData data)
        {
            // Dynamický výpočet WACC místo použití externího vstupu
            decimal wacc = CalculateWACC(data);
            
            decimal currentFCF = data.CashFlow.CurrentFCF;
            decimal initialFcfGrowth = data.Growth.PredictedFCFGrowth;
            decimal longTermGrowth = data.Growth.LongTermGrowthRate;
            decimal beta = data.MarketRisk.Beta;

            // Riziková prémie - vyšší beta, vyšší riziková prémie
            decimal riskPremium = 0.01m * (beta > 1.5m ? 0.03m : beta > 1.0m ? 0.02m : beta > 0.8m ? 0.01m : 0m);
            
            // Upravený WACC se zohledněním rizika
            decimal adjustedWacc = wacc + riskPremium;
            
            // Vícestupňový DCF model
            return CalculateMultiStageValue(data, currentFCF, initialFcfGrowth, longTermGrowth, adjustedWacc);
        }
        
        /// <summary>
        /// Dynamicky vypočítává WACC na základě kapitálové struktury, CAPM a nákladů na cizí kapitál
        /// </summary>
        private decimal CalculateWACC(FundamentalData data)
        {
            // Výchozí hodnota, pokud by výpočet selhal
            decimal defaultWacc = data.CostOfCapital.WACC;
            
            try
            {
                // Bezriziková sazba (obvykle výnos státních dluhopisů)
                decimal riskFreeRate = 0.035m; // Defaultní hodnota, ideálně by měla být z externího zdroje
                
                // Tržní riziková prémie (očekávaný výnos trhu nad bezrizikovou sazbu)
                decimal marketRiskPremium = 0.05m; // Defaultní hodnota, ideálně by měla být z externího zdroje
                
                // Beta koeficient (volatilita akcie vůči trhu)
                decimal beta = data.MarketRisk.Beta;
                
                // Výpočet nákladů na vlastní kapitál pomocí CAPM
                decimal costOfEquity = riskFreeRate + (beta * marketRiskPremium);
                
                // Náklady na cizí kapitál (úroková sazba)
                decimal costOfDebt = data.CostOfCapital.RequiredReturnOnEquity * 0.7m; // Aproximace, pokud nemáme přesná data
                
                // Daňová sazba pro úrokový daňový štít (typicky 19-21%)
                decimal taxRate = 0.21m;
                
                // Podíl vlastního kapitálu na celkovém kapitálu
                // Nutné získat z kapitálové struktury společnosti
                decimal debtToEquity = data.Stability.DebtToEquity;
                decimal equityWeight = 1.0m / (1.0m + debtToEquity);
                decimal debtWeight = 1.0m - equityWeight;
                
                // Výpočet WACC
                decimal wacc = (costOfEquity * equityWeight) + (costOfDebt * (1 - taxRate) * debtWeight);
                
                // Validace hodnoty (musí být pozitivní a v rozumném rozmezí)
                if (wacc < 0.03m) wacc = 0.03m; // Minimální hodnota
                if (wacc > 0.25m) wacc = 0.25m; // Maximální hodnota
                
                return wacc;
            }
            catch
            {
                // V případě selhání výpočtu vrátíme původní hodnotu
                return defaultWacc;
            }
        }
        
        /// <summary>
        /// Implementuje vícestupňový DCF model s různými růstovými mírami pro různé fáze
        /// </summary>
        private decimal CalculateMultiStageValue(FundamentalData data, decimal currentFCF, decimal initialGrowth, 
                                               decimal longTermGrowth, decimal adjustedWacc)
        {
            decimal presentValueSum = 0m;
            decimal discountFactor = 1 + adjustedWacc;
            
            // Přizpůsobení růstových měr podle sektoru a velikosti společnosti
            initialGrowth = AdjustGrowthRateForCompanyProperties(data, initialGrowth);
            longTermGrowth = AdjustLongTermGrowthRate(data, longTermGrowth, adjustedWacc);
            
            // Definice fází růstu
            // Fáze 1: Vysoký růst (0-5 let)
            // Fáze 2: Přechodný růst (6-10 let)
            // Fáze 3: Stabilní růst (terminální hodnota)
            int phase1Years = 5;
            int phase2Years = 5;
            int totalProjectionYears = phase1Years + phase2Years;
            
            // Fáze 1: Vysoký růst
            decimal fcf = currentFCF;
            for (int year = 1; year <= phase1Years; year++)
            {
                // V první fázi používáme počáteční růstovou míru
                fcf *= (1 + initialGrowth);
                
                // Faktor důvěry klesá s časem (méně jistoty v dlouhodobých předpovědích)
                decimal confidenceFactor = 1.0m - (0.015m * (year - 1)); 
                
                decimal discountedFCF = fcf * confidenceFactor / (decimal)Math.Pow((double)discountFactor, year);
                presentValueSum += discountedFCF;
            }
            
            // Fáze 2: Přechodný růst
            for (int year = 1; year <= phase2Years; year++)
            {
                // Lineární přechod z počáteční míry růstu na dlouhodobou míru
                decimal transitionGrowthRate = initialGrowth - 
                    ((initialGrowth - longTermGrowth) * year / phase2Years);
                
                fcf *= (1 + transitionGrowthRate);
                
                // Pokračujeme s klesajícím faktorem důvěry
                decimal confidenceFactor = 1.0m - (0.015m * (phase1Years - 1)) - (0.02m * year); 
                
                decimal discountedFCF = fcf * confidenceFactor / (decimal)Math.Pow((double)discountFactor, phase1Years + year);
                presentValueSum += discountedFCF;
            }
            
            // Fáze 3: Terminální hodnota se stabilním růstem
            decimal finalFcf = fcf * (1 + longTermGrowth);
            
            // Výpočet terminální hodnoty s konzervativnější mírou růstu
            decimal terminalValue = finalFcf / (adjustedWacc - longTermGrowth);
            
            // Aplikujeme faktor kvality dat pro terminální hodnotu
            decimal dataQualityFactor = 0.9m;
            
            decimal discountedTerminalValue = terminalValue * dataQualityFactor / 
                                          (decimal)Math.Pow((double)discountFactor, totalProjectionYears);
            
            // Aplikujeme bezpečnostní rozpětí (margin of safety)
            decimal marginOfSafety = 0.1m;
            decimal finalValue = (presentValueSum + discountedTerminalValue) * (1 - marginOfSafety);
            
            return finalValue;
        }
        
        /// <summary>
        /// Přizpůsobuje míru růstu podle vlastností společnosti a sektoru
        /// </summary>
        private decimal AdjustGrowthRateForCompanyProperties(FundamentalData data, decimal baseGrowthRate)
        {
            // Přizpůsobení podle velikosti společnosti - menší firmy mohou růst rychleji
            decimal sizeAdjustment = 0m;
            
            // Odhadujeme velikost společnosti podle SalesPerShare a aktuální ceny
            decimal estimatedMarketCap = 0m;
            if (data.Revenue.SalesPerShare > 0 && data.Price > 0)
            {
                // Jednoduchý odhad na základě P/S ratia
                decimal ps = data.Price / data.Revenue.SalesPerShare;
                decimal estimatedSize = ps * 5; // Velmi hrubý odhad relativní velikosti
                
                if (estimatedSize < 10) // Malá firma
                    sizeAdjustment = 0.02m;
                else if (estimatedSize < 30) // Střední firma
                    sizeAdjustment = 0.01m;
                else if (estimatedSize < 100) // Velká firma
                    sizeAdjustment = 0m;
                else // Velmi velká firma
                    sizeAdjustment = -0.01m;
            }
            
            // Přizpůsobení podle fáze životního cyklu (určené podle historického růstu)
            decimal cycleAdjustment = 0m;
            if (data.Growth.RevenueGrowth > 0.3m) // Rychle rostoucí
                cycleAdjustment = 0.02m;
            else if (data.Growth.RevenueGrowth > 0.1m) // Růstová
                cycleAdjustment = 0.01m;
            else if (data.Growth.RevenueGrowth > 0.03m) // Zralá
                cycleAdjustment = 0m;
            else // Pokles nebo stabilita
                cycleAdjustment = -0.01m;
            
            // Přizpůsobení podle sektoru (některé sektory mohou růst rychleji)
            // Toto by ideálně mělo být založeno na datech o růstu sektoru
            decimal sectorAdjustment = 0m;
            // Implementovat podle dostupných dat o sektorech
            
            // Aplikace přizpůsobení s kontrolou limitu
            decimal adjustedGrowthRate = baseGrowthRate + sizeAdjustment + cycleAdjustment + sectorAdjustment;
            
            // Omezení na realistické hodnoty
            adjustedGrowthRate = Math.Max(0.01m, Math.Min(0.30m, adjustedGrowthRate));
            
            return adjustedGrowthRate;
        }
        
        /// <summary>
        /// Upravuje dlouhodobou míru růstu na základě ekonomických a firemních faktorů
        /// </summary>
        private decimal AdjustLongTermGrowthRate(FundamentalData data, decimal baseLongTermGrowth, decimal wacc)
        {
            // Dlouhodobá míra růstu by měla být udržitelná a vždy nižší než WACC
            // Typicky by neměla být vyšší než dlouhodobý ekonomický růst (2-3%)
            
            // Nastavíme maximální dlouhodobou míru růstu podle WACC
            decimal maxLongTermGrowth = Math.Min(wacc * 0.6m, 0.03m);
            
            // Úprava podle ziskovosti a zadlužení
            decimal profitabilityFactor = 0m;
            if (data.Profitability.ROE > 0.2m)
                profitabilityFactor = 0.005m;
            else if (data.Profitability.ROE > 0.15m)
                profitabilityFactor = 0.003m;
            else if (data.Profitability.ROE > 0.1m)
                profitabilityFactor = 0.001m;
            
            // Faktor zadlužení (vyšší zadlužení omezuje dlouhodobý růst)
            decimal debtFactor = 0m;
            if (data.Stability.DebtToEquity > 2.0m)
                debtFactor = -0.01m;
            else if (data.Stability.DebtToEquity > 1.0m)
                debtFactor = -0.005m;
            
            // Aplikace faktorů
            decimal adjustedLongTermGrowth = baseLongTermGrowth + profitabilityFactor + debtFactor;
            
            // Omezení na maximální udržitelnou míru
            adjustedLongTermGrowth = Math.Min(adjustedLongTermGrowth, maxLongTermGrowth);
            
            // Ujistíme se, že není nižší než minimální míra
            adjustedLongTermGrowth = Math.Max(0.01m, adjustedLongTermGrowth);
            
            return adjustedLongTermGrowth;
        }
    }
}