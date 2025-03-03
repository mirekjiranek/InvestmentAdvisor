using Domain.Entities;
using System;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšený DCF model:
    /// 1. Používá předpovězené volné cash flow (FCF) s klesající mírou růstu pro příštích 10 let.
    /// 2. Diskontuje je pomocí WACC s rizikovou prémií.
    /// 3. Aplikuje faktor důvěry pro zohlednění nejistoty předpovědí.
    /// 4. Po horizontu projekce počítá terminální hodnotu (Gordon Growth Model).
    /// 5. Výsledek je současná hodnota všech budoucích toků.
    /// </summary>
    public class DCFModel
    {
        public decimal CalculateDCFValue(FundamentalData data)
        {
            // Základní parametry modelu
            decimal wacc = data.CostOfCapital.WACC;
            decimal currentFCF = data.CashFlow.CurrentFCF;
            decimal initialFcfGrowth = data.Growth.PredictedFCFGrowth;
            decimal longTermGrowth = data.Growth.LongTermGrowthRate;
            decimal beta = data.MarketRisk.Beta;

            // Riziková prémie - vyšší beta, vyšší riziková prémie
            decimal riskPremium = 0.01m * (beta > 1.5m ? 0.02m : beta > 1.0m ? 0.01m : 0m);
            
            // Upravený WACC se zohledněním rizika
            decimal adjustedWacc = wacc + riskPremium;
            
            int projectionYears = 10;
            decimal discountFactor = 1 + adjustedWacc;
            decimal presentValueSum = 0m;

            // Diskontujeme budoucí FCF pro 10 let s postupně klesajícím růstem
            decimal fcf = currentFCF;
            for (int year = 1; year <= projectionYears; year++)
            {
                // Postupně snižujeme míru růstu směrem k dlouhodobému růstu
                decimal currentGrowthRate = initialFcfGrowth * (1 - ((decimal)year / (projectionYears * 2))) + 
                                           (longTermGrowth * ((decimal)year / (projectionYears * 2)));
                
                fcf *= (1 + currentGrowthRate);
                
                // Faktor důvěry klesá s časem (méně jistoty v dlouhodobých předpovědích)
                decimal confidenceFactor = 1.0m - (0.02m * (year - 1)); 
                
                decimal discountedFCF = fcf * confidenceFactor / (decimal)Math.Pow((double)discountFactor, year);
                presentValueSum += discountedFCF;
            }

            // Vylepšený výpočet terminální hodnoty
            decimal finalFcf = fcf * (1 + longTermGrowth);
            
            // Kontrola realističnosti dlouhodobé míry růstu
            decimal effectiveLongTermGrowth = Math.Min(longTermGrowth, adjustedWacc * 0.6m);
            
            // Výpočet terminální hodnoty s konzervativnější mírou růstu
            decimal terminalValue = finalFcf / (adjustedWacc - effectiveLongTermGrowth);
            
            // Aplikujeme faktor kvality dat pro terminální hodnotu
            decimal dataQualityFactor = 0.9m; // defaultní hodnota
            
            decimal discountedTerminalValue = terminalValue * dataQualityFactor / 
                                             (decimal)Math.Pow((double)discountFactor, projectionYears);

            // Aplikujeme bezpečnostní rozpětí (margin of safety)
            decimal marginOfSafety = 0.1m;
            decimal finalValue = (presentValueSum + discountedTerminalValue) * (1 - marginOfSafety);

            return finalValue;
        }
    }
}