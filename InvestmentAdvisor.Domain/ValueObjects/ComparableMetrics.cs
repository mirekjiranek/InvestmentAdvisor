namespace Domain.ValueObjects
{
    /// <summary>
    /// Metriky pro srovnání s ostatními společnostmi (Comparable Analysis).
    /// </summary>
    public record ComparableMetrics(
        decimal SectorAveragePE,       // Průměrný P/E v sektoru
        decimal SectorMedianPB,        // Medián P/B sektoru
        decimal PeerEVEBITDA,          // Průměrný EV/EBITDA peers
        decimal SectorPriceSales       // Průměrný Price/Sales sektoru
    );
}
