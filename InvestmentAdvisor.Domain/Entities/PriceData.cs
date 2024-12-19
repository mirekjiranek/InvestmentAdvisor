using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Představuje cenový záznam pro daný ticker v určitém čase (např. denní OHLC data)
    /// </summary>
    public class PriceData
    {
        public Guid Id { get; private set; }
        public Guid InvestmentInstrumentId { get; private set; }

        public DateTime Date { get; private set; }
        public decimal Open { get; private set; }
        public decimal High { get; private set; }
        public decimal Low { get; private set; }
        public decimal Close { get; private set; }
        public long Volume { get; private set; }

        protected PriceData() { }

        public PriceData(DateTime date, decimal open, decimal high, decimal low, decimal close, long volume)
        {
            Id = Guid.NewGuid();
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }
}
