using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    // Dotazy vrací data, nemění stav.
    public interface IQuery<TResult>
    {
        // Dotazy definují typ výsledku (TResult), který vrátí.
        // Díky tomu handler ví, co má vrátit.
    }
}
