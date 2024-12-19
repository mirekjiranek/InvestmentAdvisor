using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    // Interfaces/ICommandHandler.cs
    // Handler zpracovávající příkaz.
    // Oddělení příkazu od handleru umožňuje lepší testovatelnost a udržitelnost.
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
        // HandleAsync zpracovává příkaz a nic nevrací, protože cílem příkazu je změna stavu.
    }
}
