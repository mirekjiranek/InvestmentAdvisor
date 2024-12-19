namespace Application.Commands
{
    // Commands/UpdateInstrumentsCommand.cs
    // Příkaz, který říká aplikaci: "Aktualizuj data pro uvedené instrumenty." 
    // Např. stáhne nové fundamentální a cenové údaje z externích API.
    // Tento příkaz nemá návratový typ (void), nebo spíše Task bez hodnoty.
    public class UpdateInstrumentsCommand : ICommand
    {
        // Parametry příkazu - např. seznam symbolů, které chceme aktualizovat
        public List<string> SymbolsToUpdate { get; }

        public UpdateInstrumentsCommand(List<string> symbols)
        {
            SymbolsToUpdate = symbols;
        }
    }
}
