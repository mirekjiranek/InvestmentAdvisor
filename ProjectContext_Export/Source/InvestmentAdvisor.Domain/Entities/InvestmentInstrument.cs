using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Reprezentuje investiční nástroj (např. akcie, ETF, dluhopisy).
    /// </summary>
    public class InvestmentInstrument
    {
        public Guid InvestmentInstrumentId { get; private set; }

        /// <summary>
        /// Symbol instrumentu (např. "AAPL", "SPY", "TSLA").
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Název společnosti nebo ETF.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Fundamentální data související s instrumentem.
        /// </summary>
        public FundamentalData FundamentalData { get; private set; }

        /// <summary>
        /// Historie cenových dat (OHLC).
        /// </summary>
        public List<PriceData> PriceHistory { get; private set; } = new List<PriceData>();

        /// <summary>
        /// Aktuální doporučení na základě modelů.
        /// </summary>
        public Recommendation CurrentRecommendation { get; private set; }

        protected InvestmentInstrument() { }

        public InvestmentInstrument(string symbol, string name)
        {
            InvestmentInstrumentId = Guid.NewGuid();
            Symbol = symbol;
            Name = name;
        }

        public void UpdateFundamentalData(FundamentalData newData)
        {
            FundamentalData = newData;
        }

        public void SetRecommendation(Recommendation recommendation)
        {
            CurrentRecommendation = recommendation;
        }

        public void AddPriceData(PriceData priceData)
        {
            PriceHistory.Add(priceData);
        }
    }
}
