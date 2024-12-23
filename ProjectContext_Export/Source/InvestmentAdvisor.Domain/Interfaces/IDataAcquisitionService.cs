using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// Rozhraní pro službu zajišťující sběr, transformaci a uložení dat
    /// z externích finančních API do databáze, a jejich aktualizaci.
    /// </summary>
    public interface IDataAcquisitionService
    {
        /// <summary>
        /// Provede kompletní aktualizaci (cenová data, fundamentální data, sentiment)
        /// pro daný symbol.
        /// </summary>
        Task FullUpdateAsync(string symbol);

        /// <summary>
        /// Aktualizuje cenová data pro daný symbol.
        /// </summary>
        Task UpdatePriceDataAsync(string symbol);

        /// <summary>
        /// Aktualizuje fundamentální data pro daný symbol.
        /// </summary>
        Task UpdateFundamentalsAsync(string symbol);

        /// <summary>
        /// Aktualizuje sentiment a odhady analytiků pro daný symbol.
        /// </summary>
        Task UpdateSentimentAndEstimatesAsync(string symbol);
    }
}
