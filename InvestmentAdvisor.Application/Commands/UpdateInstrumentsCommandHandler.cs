using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands
{
    // Commands/UpdateInstrumentsCommandHandler.cs
    // Handler, který zpracuje příkaz UpdateInstrumentsCommand.
    // Proč v Application? Protože Application orchestruje logiku:
    // - Zavolá doménové služby, repozitáře (přes rozhraní definované v Domain)
    // - Nepřistupuje přímo k db detailům (to obstará Infrastructure přes repozitář)
    // - Tím oddělujeme logiku z Domain a Infrastructure od prezentační vrstvy
    //
    // Tento handler využije například IInvestmentInstrumentRepository a DataAcquisitionService
    // z Infrastructure, injektované přes DI. Ale pozor, Application projekt by NEMĚL referencovat Infrastructure!
    // Místo toho Application zná jen rozhraní z Domain a případně use-case rozhraní.
    // Konkrétní implementace jsou zaregistrovány přes DI v Composition Root (např. v Web).
    public class UpdateInstrumentsCommandHandler : ICommandHandler<UpdateInstrumentsCommand>
    {
        private readonly IDataAcquisitionService _dataAcquisitionService;
        // IDataAcquisitionService je hypotetické rozhraní definované v Domain nebo Application
        // Pokud to není, měli bychom ho tam přidat. Cílem je, aby Application nemusela referencovat Infrastructure přímo.

        public UpdateInstrumentsCommandHandler(IDataAcquisitionService dataAcquisitionService)
        {
            _dataAcquisitionService = dataAcquisitionService;
        }

        public async Task HandleAsync(UpdateInstrumentsCommand command, CancellationToken cancellationToken = default)
        {
            // Využijeme novou metodu UpdateInstrumentsDataAsync, která zpracuje seznam symbolů najednou
            await _dataAcquisitionService.UpdateInstrumentsDataAsync(command.SymbolsToUpdate, cancellationToken);
        }
    }

}