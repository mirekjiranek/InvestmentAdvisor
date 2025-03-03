using Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// Rozhraní pro repozitář investičních nástrojů (akcie, ETF, dluhopisy...).
    /// Umožňuje specializované dotazy nad rámec základních CRUD operací.
    /// </summary>
    public interface IInvestmentInstrumentRepository : IRepository<InvestmentInstrument>
    {
        /// <summary>
        /// Získá investiční nástroj podle jeho symbolu (např. "AAPL").
        /// Pokud neexistuje, vrátí null.
        /// </summary>
        Task<InvestmentInstrument?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
    }
}