using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    // ICommand nevrací hodnotu, příkazy jsou určeny ke změně stavu.
    public interface ICommand
    {
        // Toto rozhraní je prázdné, slouží jen k označení objektů jako "Command".
        // Díky tomu lze snadno najít všechny příkazy a pomocí konvencí je registrovat.
    }
}
