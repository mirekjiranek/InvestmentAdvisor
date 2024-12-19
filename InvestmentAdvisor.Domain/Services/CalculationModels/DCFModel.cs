using Domain.Entities;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Propracovanější DCF model:
    /// 1. Používá předpovězené volné cash flow (FCF) pro příštích 5-10 let.
    /// 2. Diskontuje je pomocí WACC.
    /// 3. Po horizontu projekce počítá terminální hodnotu (Gordon Growth Model).
    /// 4. Výsledek je současná hodnota všech budoucích toků.
    /// </summary>
    public class DCFModel
    {
        public decimal CalculateDCFValue(FundamentalData data)
        {
            // Předpokládejme, že `data` má:
            // - data.Growth.PredictedFCFGrowth: odhad růstu FCF
            // - data.CostOfCapital.WACC: vážený průměr nákladů kapitálu
            // - data.CurrentFCF: aktuální roční free cash flow
            // - data.LongTermGrowthRate: dlouhodobý udržitelný růst po horizontu 10 let

            decimal wacc = data.CostOfCapital.WACC;
            decimal currentFCF = data.CashFlow.CurrentFCF;
            decimal fcfGrowth = data.Growth.PredictedFCFGrowth;
            decimal longTermGrowth = data.Growth.LongTermGrowthRate;

            int projectionYears = 10;
            decimal discountFactor = 1 + wacc;
            decimal presentValueSum = 0m;

            // Diskontujeme budoucí FCF pro 10 let
            decimal fcf = currentFCF;
            for (int year = 1; year <= projectionYears; year++)
            {
                fcf *= (1 + fcfGrowth); // každý rok roste FCF
                decimal discountedFCF = fcf / (decimal)Math.Pow((double)discountFactor, year);
                presentValueSum += discountedFCF;
            }

            // Terminální hodnota po 10 letech (Gordon Growth Model)
            decimal terminalValue = (fcf * (1 + longTermGrowth)) / (wacc - longTermGrowth);
            decimal discountedTerminalValue = terminalValue / (decimal)Math.Pow((double)discountFactor, projectionYears);

            return presentValueSum + discountedTerminalValue;
        }
    }
}
