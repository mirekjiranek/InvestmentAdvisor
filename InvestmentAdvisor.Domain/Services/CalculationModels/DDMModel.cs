using Domain.Entities;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Dividend Discount Model (DDM) pro firmy s pravidelnou dividendou:
    /// P = D1 / (r - g)
    /// D1 = očekávaná dividenda za rok
    /// r = diskontní sazba (např. WACC nebo požadovaná míra návratnosti)
    /// g = dlouhodobý růst dividend
    /// </summary>
    public class DDMModel
    {
        public decimal CalculateDDMValue(FundamentalData data)
        {
            decimal r = data.CostOfCapital.RequiredReturnOnEquity; // např. požadovaná návratnost
            decimal g = data.Dividend.DividendGrowth;
            decimal currentDividend = data.Dividend.CurrentAnnualDividend; // Aktuální roční dividenda

            // Odhad dividendy za rok (D1)
            decimal D1 = currentDividend * (1 + g);

            // Kontrola, pokud je g >= r, model nedává smysl. Musíme to ošetřit:
            if (g >= r)
            {
                // V takovém případě je model nepoužitelný, vrátíme např. základní odhad
                return currentDividend / r;
            }

            return D1 / (r - g);
        }
    }
}
