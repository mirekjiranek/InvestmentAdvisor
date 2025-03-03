using Domain.Entities;
using System;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšený Dividend Discount Model (DDM) pro firmy s pravidelnou dividendou.
    /// Používá třífázový model:
    /// 1. Počáteční fáze rychlejšího růstu
    /// 2. Přechodná fáze
    /// 3. Stabilní fáze dlouhodobého udržitelného růstu
    /// 
    /// Zohledňuje také:
    /// - Udržitelnost výplatního poměru
    /// - Vztah mezi dividendovým růstem a růstem EPS
    /// </summary>
    public class DDMModel
    {
        public decimal CalculateDDMValue(FundamentalData data)
        {
            // Základní vstupy
            decimal r = data.CostOfCapital.RequiredReturnOnEquity;
            decimal initialGrowth = data.Dividend.DividendGrowth;
            decimal longTermGrowth = data.Growth.LongTermGrowthRate;
            decimal currentDividend = data.Dividend.CurrentAnnualDividend;
            decimal epsGrowth = data.Growth.PredictedEpsGrowth;
            decimal payoutRatio = data.Dividend.PayoutRatio;

            // Pokud společnost nevyplácí dividendu nebo má extrémně nízký payout ratio, model není vhodný
            if (currentDividend <= 0 || payoutRatio < 0.1m)
            {
                return 0; // Model není relevantní, signalizujeme nulovou váhu
            }

            // Kontrola udržitelnosti dividendového růstu vzhledem k růstu zisků
            // Dividenda nemůže dlouhodobě růst rychleji než zisky
            decimal sustainableGrowth = Math.Min(initialGrowth, epsGrowth * 1.1m);
            
            // Kontrola realističnosti dlouhodobé míry růstu
            decimal effectiveLongTermGrowth = Math.Min(longTermGrowth, r * 0.67m);
            
            // Počet let v první (rychlejší) fázi růstu
            int fastGrowthYears = 5;
            // Počet let v přechodné fázi
            int transitionYears = 5;
            
            decimal value = 0;
            decimal discountFactor = 1 + r;
            
            // 1. Fáze - počáteční rychlejší růst
            decimal dividend = currentDividend;
            for (int year = 1; year <= fastGrowthYears; year++)
            {
                dividend *= (1 + sustainableGrowth);
                value += dividend / (decimal)Math.Pow((double)discountFactor, year);
            }
            
            // 2. Fáze - přechodný růst (lineární pokles z rychlého růstu na dlouhodobý)
            for (int year = 1; year <= transitionYears; year++)
            {
                decimal transitionGrowth = sustainableGrowth - 
                    ((sustainableGrowth - effectiveLongTermGrowth) * year / transitionYears);
                    
                dividend *= (1 + transitionGrowth);
                value += dividend / (decimal)Math.Pow((double)discountFactor, fastGrowthYears + year);
            }
            
            // 3. Fáze - terminální hodnota s dlouhodobým růstem
            decimal terminalDividend = dividend * (1 + effectiveLongTermGrowth);
            decimal terminalValue = terminalDividend / (r - effectiveLongTermGrowth);
            
            // Aplikujeme faktor udržitelnosti výplatního poměru
            decimal sustainabilityFactor = payoutRatio > 0.8m ? 0.9m : 1.0m;
            
            decimal discountedTerminalValue = terminalValue * sustainabilityFactor / 
                (decimal)Math.Pow((double)discountFactor, fastGrowthYears + transitionYears);
            
            return value + discountedTerminalValue;
        }
    }
}