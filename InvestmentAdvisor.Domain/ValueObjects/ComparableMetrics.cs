namespace Domain.ValueObjects
{
    /// <summary>
    /// Metriky pro srovnání s ostatními společnostmi (Comparable Analysis).
    /// </summary>
    public record ComparableMetrics(
        decimal SectorAveragePE,       // Průměrný P/E v sektoru
        decimal SectorMedianPB,        // Medián P/B sektoru
        decimal PeerEVEBITDA,          // Průměrný EV/EBITDA peers
        decimal SectorPriceSales,      // Průměrný Price/Sales sektoru
        decimal SectorForwardPE = 0,   // Průměrný Forward P/E v sektoru
        decimal SectorPriceBook = 0,   // Průměrný P/B v sektoru
        decimal SectorPEG = 0,         // Průměrný PEG ratio sektoru
        decimal SectorAverageROE = 0,  // Průměrné ROE sektoru
        decimal SectorAverageNetMargin = 0, // Průměrná čistá marže sektoru
        decimal SectorEVSales = 0,     // Průměrný EV/Sales sektoru
        decimal SectorPriceFCF = 0,    // Průměrný Price/FCF sektoru
        decimal SectorAverageGrowth = 0 // Průměrný růst zisků v sektoru
    );
}