using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Rozšířené doporučení s více časovými horizonty a odvětvovým kontextem.
    /// </summary>
    public class Recommendation
    {
        public Guid Id { get; private set; }
        public Guid InvestmentInstrumentId { get; private set; }

        /// <summary>
        /// Navigační vlastnost na InvestmentInstrument.
        /// </summary>
        public InvestmentInstrument InvestmentInstrument { get; private set; }

        /// <summary>
        /// Akce doporučení v rozšířené formě.
        /// </summary>
        public RecommendationAction Action { get; private set; }

        /// <summary>
        /// Číselné skóre odpovídající doporučení (např. 1 = StrongBuy, 7 = StrongSell)
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Časový horizont (např. "Krátkodobý", "Střednědobý", "Dlouhodobý")
        /// </summary>
        public string TimeHorizon { get; private set; }
        
        /// <summary>
        /// Alias pro TimeHorizon - používaný v některých modelech
        /// </summary>
        public string TimePeriod => TimeHorizon;

        /// <summary>
        /// Cílová cena, kam by se akcie/ETF mohla dostat v rámci daného horizontu.
        /// </summary>
        public decimal TargetPrice { get; private set; }

        /// <summary>
        /// Stručné vysvětlení doporučení (proč takové doporučení padlo).
        /// </summary>
        public string Rationale { get; private set; }

        /// <summary>
        /// Úroveň rizika (např. "Nízké", "Střední", "Vysoké")
        /// </summary>
        public string RiskLevel { get; private set; }
        
        /// <summary>
        /// Postavení společnosti v sektoru (např. "Leader", "Average", "Laggard")
        /// </summary>
        public string SectorPosition { get; private set; }
        
        /// <summary>
        /// Krátkodobý výhled (1-3 měsíce)
        /// </summary>
        public string ShortTermOutlook { get; private set; }
        
        /// <summary>
        /// Střednědobý výhled (6-12 měsíců)
        /// </summary>
        public string MidTermOutlook { get; private set; }
        
        /// <summary>
        /// Dlouhodobý výhled (2-3 roky)
        /// </summary>
        public string LongTermOutlook { get; private set; }

        protected Recommendation() { }

        public Recommendation(
            RecommendationAction action,
            int score,
            string timeHorizon,
            decimal targetPrice,
            string rationale,
            string riskLevel,
            string sectorPosition = "",
            string shortTermOutlook = "",
            string midTermOutlook = "",
            string longTermOutlook = "")
        {
            Id = Guid.NewGuid();
            Action = action;
            Score = score;
            TimeHorizon = timeHorizon;
            TargetPrice = targetPrice;
            Rationale = rationale;
            RiskLevel = riskLevel;
            SectorPosition = sectorPosition;
            ShortTermOutlook = shortTermOutlook;
            MidTermOutlook = midTermOutlook;
            LongTermOutlook = longTermOutlook;
        }
    }

    public enum RecommendationAction
    {
        StrongBuy = 1,
        Buy = 2,
        Accumulate = 3,
        Hold = 4,
        Reduce = 5,
        Sell = 6,
        StrongSell = 7
    }
}