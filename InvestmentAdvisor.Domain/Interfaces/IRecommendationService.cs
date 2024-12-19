using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Služba pro generování doporučení k nákupu/prodeji/držení investičního nástroje na základě vnitřní hodnoty, tržní ceny a dalších metrik.
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Generuje doporučení (např. StrongBuy, Buy, Accumulate, Hold, Reduce, Sell, StrongSell),
        /// včetně skóre, časového horizontu, cílové ceny, důvodu a rizikového profilu.
        /// </summary>
        /// <param name="instrument">Investiční nástroj s načtenými fundamentálními a cenovými daty.</param>
        /// <returns>Objekt Recommendation s detailními informacemi.</returns>
        Recommendation GenerateRecommendation(InvestmentInstrument instrument);
    }
}
