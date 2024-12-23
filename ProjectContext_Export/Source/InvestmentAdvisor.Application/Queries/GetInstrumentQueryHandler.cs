using Application.DTOs;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    // Queries/GetInstrumentQueryHandler.cs
    // Handler zpracovávající dotaz GetInstrumentQuery.
    // Proč v Application? Protože opět orchestrujeme logiku: použijeme repozitář 
    // (skrze doménové rozhraní IInvestmentInstrumentRepository) k načtení dat a namapujeme do DTO.
    //
    // Application by měla záviset pouze na Domain (rozhraní repozitáře), 
    // nikoliv na konkrétní implementaci v Infrastructure (ta se dočte z DI).
    public class GetInstrumentQueryHandler : IQueryHandler<GetInstrumentQuery, InstrumentDto?>
    {
        private readonly IInvestmentInstrumentRepository _repository;

        public GetInstrumentQueryHandler(IInvestmentInstrumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<InstrumentDto?> HandleAsync(GetInstrumentQuery query, CancellationToken cancellationToken = default)
        {
            // Načteme instrument z repozitáře:
            var instrument = await _repository.GetBySymbolAsync(query.Symbol);

            if (instrument == null)
                return null;

            // Přemapujeme doménový model (InvestmentInstrument) na InstrumentDto
            // Application vrstva se stará o mapování dat na DTO, aby UI nebo API vrstva dostaly čistý objekt.
            var lastPrice = instrument.PriceHistory.OrderByDescending(p => p.Date).FirstOrDefault()?.Close;
            var recommendation = instrument.CurrentRecommendation?.Action.ToString();

            return new InstrumentDto
            {
                Id = instrument.InvestmentInstrumentId,
                Symbol = instrument.Symbol,
                Name = instrument.Name,
                LastPrice = lastPrice,
                CurrentRecommendation = recommendation
            };
        }
    }

}
