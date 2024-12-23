using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    // Queries/GetInstrumentQuery.cs
    // Dotaz, který říká: "Dej mi informace o konkrétním instrumentu."
    // Dotaz by měl vracet např. InstrumentDto s agregovanými daty.
    public class GetInstrumentQuery : IQuery<InstrumentDto?>
    {
        public string Symbol { get; }

        public GetInstrumentQuery(string symbol)
        {
            Symbol = symbol;
        }
    }

}
