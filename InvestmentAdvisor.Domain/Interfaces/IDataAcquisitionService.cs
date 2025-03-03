using System;
using System.Collections.Generic;
using System.Threading;
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
        Task FullUpdateAsync(string symbol, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Aktualizuje data pro seznam symbolů.
        /// </summary>
        Task UpdateInstrumentsDataAsync(List<string> symbols, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aktualizuje cenová data pro daný symbol.
        /// </summary>
        Task UpdatePriceDataAsync(string symbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aktualizuje fundamentální data pro daný symbol.
        /// </summary>
        Task UpdateFundamentalsAsync(string symbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aktualizuje sentiment a odhady analytiků pro daný symbol.
        /// </summary>
        Task UpdateSentimentAndEstimatesAsync(string symbol, CancellationToken cancellationToken = default);
    }
}