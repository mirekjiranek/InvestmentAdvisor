using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// Služba pro výpočet vnitřní hodnoty investičního nástroje na základě fundamentálních dat a modelů.
    /// </summary>
    public interface IValuationService
    {
        /// <summary>
        /// Vypočítá vnitřní hodnotu investičního nástroje (např. férovou cenu).
        /// Tato hodnota je využívána při generování doporučení.
        /// </summary>
        /// <param name="instrument">Investiční nástroj (akcie, ETF...)</param>
        /// <returns>Numerická hodnota reprezentující odhadovanou vnitřní cenu.</returns>
        decimal CalculateIntrinsicValue(InvestmentInstrument instrument);
    }
}
