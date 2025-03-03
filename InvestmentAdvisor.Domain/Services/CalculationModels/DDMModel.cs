using Domain.Entities;
using System;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Vylepšený Dividend Discount Model (DDM) pro firmy s pravidelnou dividendou.
    /// Používá vícestupňový model s různými fázemi růstu:
    /// 1. Počáteční fáze rychlejšího růstu
    /// 2. Přechodná fáze
    /// 3. Stabilní fáze dlouhodobého udržitelného růstu
    /// 
    /// Zohledňuje také:
    /// - Udržitelnost výplatního poměru
    /// - Vztah mezi dividendovým růstem a růstem EPS
    /// - Zpětné odkupy akcií jako alternativní formu odměny akcionářům
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
            
            // Odhad zpětných odkupů akcií, pokud společnost provádí buybacky
            decimal estimatedBuybackYield = EstimateBuybackYield(data);
            
            // Kombinovaný výplatní výnos (dividenda + buyback)
            decimal totalYield = (currentDividend / data.Price) + estimatedBuybackYield;
            
            // Pokud společnost nevyplácí dividendu ani neprovádí zpětné odkupy, model není vhodný
            if (totalYield <= 0.001m)
            {
                return 0; // Model není relevantní, signalizujeme nulovou váhu
            }
            
            // Pokud společnost neprovádí dividendy, ale dělá buybacky, používáme upravenou metodiku
            if (currentDividend <= 0 && estimatedBuybackYield > 0)
            {
                return CalculateTotalShareholderReturnValue(data, estimatedBuybackYield, r, epsGrowth, longTermGrowth);
            }

            // Kontrola udržitelnosti dividendového růstu vzhledem k růstu zisků
            // Dividenda nemůže dlouhodobě růst rychleji než zisky
            decimal sustainableGrowth = Math.Min(initialGrowth, epsGrowth * 1.1m);
            
            // Kontrola realističnosti dlouhodobé míry růstu
            decimal effectiveLongTermGrowth = Math.Min(longTermGrowth, r * 0.67m);
            
            // Vícestupňový model s proměnlivým počtem fází podle specifik společnosti
            return CalculateMultiStageDividendValue(data, currentDividend, sustainableGrowth, 
                                                  effectiveLongTermGrowth, r, estimatedBuybackYield);
        }
        
        /// <summary>
        /// Odhaduje efektivní výnos ze zpětných odkupů akcií na základě dostupných dat
        /// </summary>
        private decimal EstimateBuybackYield(FundamentalData data)
        {
            // Ve skutečném modelu by tento výpočet byl založen na historických datech o buybackech,
            // změnách v počtu akcií, cash flow dostupného pro akcionáře atd.
            
            // Zjednodušený odhad založený na:
            // 1. Nízkém výplatním poměru
            // 2. Vysokém FCF
            // 3. Nízké míře růstu aktiv (indikuje, že hotovost není reinvestována)
            
            decimal estimatedBuybackYield = 0m;
            
            // Pokud společnost generuje silné FCF, ale má nízký výplatní poměr,
            // je pravděpodobné, že provádí zpětné odkupy
            if (data.Dividend.PayoutRatio < 0.3m && data.CashFlow.FreeCashFlowYield > 0.05m)
            {
                // Odhadujeme, že část FCF nad rámec dividend jde na buybacky
                decimal potentialBuybackCash = data.CashFlow.FreeCashFlowYield - 
                                              (data.Dividend.CurrentAnnualDividend / data.Price);
                
                // Předpokládáme, že jen část dostupné hotovosti jde na buybacky
                estimatedBuybackYield = potentialBuybackCash * 0.5m;
                
                // Limitujeme odhad na realistické hodnoty
                estimatedBuybackYield = Math.Min(estimatedBuybackYield, 0.05m);
            }
            
            return estimatedBuybackYield;
        }
        
        /// <summary>
        /// Počítá hodnotu pomocí algoritmu přizpůsobeného počtu fází a charakteristikám společnosti
        /// </summary>
        private decimal CalculateMultiStageDividendValue(FundamentalData data, decimal currentDividend, 
                                                       decimal sustainableGrowth, decimal longTermGrowth, 
                                                       decimal r, decimal buybackYield)
        {
            // Určíme optimální strukturu fází podle charakteristik společnosti
            int fastGrowthYears;
            int transitionYears;
            
            // Mladší, rychle rostoucí společnosti mohou mít delší fázi rychlého růstu
            if (sustainableGrowth > 0.15m)
            {
                fastGrowthYears = 7;
                transitionYears = 5;
            }
            // Střední tempo růstu
            else if (sustainableGrowth > 0.08m)
            {
                fastGrowthYears = 5;
                transitionYears = 5;
            }
            // Pomalejší růst - typicky zralé společnosti
            else
            {
                fastGrowthYears = 3;
                transitionYears = 4;
            }
            
            decimal value = 0;
            decimal discountFactor = 1 + r;
            decimal totalReturnFactor = 1.0m;
            
            // Pokud společnost kombinuje dividendy a buybacky, upravíme ohodnocení
            if (buybackYield > 0)
            {
                // Buybacky zvyšují hodnotu na akcii pro zbývající akcionáře
                // Zjednodušený model: předpokládáme, že buyback zvyšuje EPS a tím i budoucí dividendy
                totalReturnFactor = 1.0m + (buybackYield / (data.Dividend.CurrentAnnualDividend / data.Price) * 0.3m);
                
                // Limitujeme faktor na rozumné hodnoty
                totalReturnFactor = Math.Min(totalReturnFactor, 1.3m);
            }
            
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
                    ((sustainableGrowth - longTermGrowth) * year / transitionYears);
                    
                dividend *= (1 + transitionGrowth);
                value += dividend / (decimal)Math.Pow((double)discountFactor, fastGrowthYears + year);
            }
            
            // 3. Fáze - terminální hodnota s dlouhodobým růstem
            decimal terminalDividend = dividend * (1 + longTermGrowth);
            decimal terminalValue = terminalDividend / (r - longTermGrowth);
            
            // Aplikujeme faktor udržitelnosti výplatního poměru
            decimal sustainabilityFactor = data.Dividend.PayoutRatio > 0.8m ? 0.9m : 1.0m;
            
            decimal discountedTerminalValue = terminalValue * sustainabilityFactor / 
                (decimal)Math.Pow((double)discountFactor, fastGrowthYears + transitionYears);
            
            // Aplikujeme korekci na celkový výnos (dividend + buyback)
            return (value + discountedTerminalValue) * totalReturnFactor;
        }
        
        /// <summary>
        /// Alternativní model pro společnosti, které místo dividend primárně využívají zpětné odkupy akcií
        /// </summary>
        private decimal CalculateTotalShareholderReturnValue(FundamentalData data, decimal buybackYield, 
                                                          decimal r, decimal epsGrowth, decimal longTermGrowth)
        {
            // Pro společnosti, které upřednostňují buybacky před dividendami,
            // použijeme model založený na celkovém výnosu akcionářů
            
            // Základní předpoklady:
            // 1. Zpětné odkupy zvyšují EPS a tím i hodnotu pro zbývající akcionáře
            // 2. Míra buybacků je omezena disponibilním cash flow
            // 3. Dlouhodobě je tempo buybacků limitováno růstem společnosti
            
            decimal currentPrice = data.Price;
            decimal impliedTotalYield = buybackYield; // Současný "výnos" z buybacků
            
            // Odhadujeme dobu trvání současné míry buybacků
            int highBuybackYears = 5;
            int transitionYears = 5;
            decimal sustainableBuybackRate = Math.Min(longTermGrowth + 0.01m, buybackYield * 0.5m);
            
            decimal value = 0;
            decimal discountFactor = 1 + r;
            decimal yearlyBenefit = currentPrice * impliedTotalYield;
            
            // Fáze 1: Současná vysoká míra zpětných odkupů
            for (int year = 1; year <= highBuybackYears; year++)
            {
                // Předpokládáme, že hodnota zpětných odkupů roste s EPS
                yearlyBenefit *= (1 + epsGrowth * 0.8m);
                value += yearlyBenefit / (decimal)Math.Pow((double)discountFactor, year);
            }
            
            // Fáze 2: Přechod k dlouhodobě udržitelné míře
            for (int year = 1; year <= transitionYears; year++)
            {
                decimal transitionRate = epsGrowth * 0.8m - 
                    ((epsGrowth * 0.8m - sustainableBuybackRate) * year / transitionYears);
                    
                yearlyBenefit *= (1 + transitionRate);
                value += yearlyBenefit / (decimal)Math.Pow((double)discountFactor, highBuybackYears + year);
            }
            
            // Fáze 3: Terminální hodnota s dlouhodobě udržitelnou mírou
            decimal terminalBenefit = yearlyBenefit * (1 + sustainableBuybackRate);
            decimal terminalValue = terminalBenefit / (r - sustainableBuybackRate);
            
            decimal discountedTerminalValue = terminalValue / 
                (decimal)Math.Pow((double)discountFactor, highBuybackYears + transitionYears);
            
            // Aplikujeme korekci na udržitelnost míry buybacků
            decimal sustainabilityFactor = 0.9m; // Buybacky jsou méně předvídatelné než dividendy
            
            return (value + discountedTerminalValue) * sustainabilityFactor;
        }
    }
}