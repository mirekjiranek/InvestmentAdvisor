using Domain.Entities;

namespace Domain.Services.CalculationModels
{
    /// <summary>
    /// Srovnávací analýza:
    /// Porovnáme valuační násobky firmy (P/E, EV/EBITDA, P/S) s průměry a mediány sektoru.
    /// Pak z těchto porovnání odvodíme odhad férové ceny:
    /// - Např. férová cena = průměr z odhadů P/E-based price, EV/EBITDA-based price, atd.
    /// </summary>
    public class ComparableAnalysisModel
    {
        public decimal CalculateComparableValue(FundamentalData data)
        {
            // Potřebujeme:
            // - Valuační ukazatele společnosti (PE, EV/EBITDA, PriceSales)
            // - Průměrné ukazatele sektoru (SectorAveragePE, SectorMedianEVEBITDA, SectorAvgPriceSales)
            // - EPS společnosti, EBITDA, Sales/Revenue per share pro výpočet vnitřní ceny

            decimal companyEPS = data.Earnings.EPS;
            decimal companyEBITDA = data.Earnings.EBITDA;
            decimal companySalesPerShare = data.Revenue.SalesPerShare;

            // Sektorové průměry (z ComparableMetrics)
            decimal sectorPE = data.Comparable.SectorAveragePE;
            decimal sectorEVEBITDA = data.Comparable.PeerEVEBITDA;
            decimal sectorPriceSales = data.Comparable.SectorPriceSales; 

            // Odhad férové ceny z P/E
            decimal fairValuePE = companyEPS * sectorPE;

            // Odhad férové ceny z EV/EBITDA (zjednodušeně použijeme EBITDA a předpokládejme poměr EV=Price)
            // V reálu bychom potřebovali i zadlužení k výpočtu EV, zde jen aproximace:
            decimal fairValueEVEBITDA = (companyEBITDA * sectorEVEBITDA);

            // Odhad férové ceny z Price/Sales
            decimal fairValuePS = companySalesPerShare * sectorPriceSales;

            // Průměr těchto odhadů
            decimal averageFairValue = (fairValuePE + fairValueEVEBITDA + fairValuePS) / 3m;

            return averageFairValue;
        }
    }

}
