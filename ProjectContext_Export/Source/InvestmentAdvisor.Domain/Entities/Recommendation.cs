using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Rozšířené doporučení.
    /// </summary>
    public class Recommendation
    {
        public Guid Id { get; private set; }
        public Guid InvestmentInstrumentId { get; private set; }

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

        protected Recommendation() { }

        public Recommendation(
            RecommendationAction action,
            int score,
            string timeHorizon,
            decimal targetPrice,
            string rationale,
            string riskLevel)
        {
            Id = Guid.NewGuid();
            Action = action;
            Score = score;
            TimeHorizon = timeHorizon;
            TargetPrice = targetPrice;
            Rationale = rationale;
            RiskLevel = riskLevel;
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
