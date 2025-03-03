using Domain.ValueObjects;
using System;

namespace Domain.Entities
{
    /// <summary>
    /// Třída obsahující všechny fundamentální ukazatele potřebné pro analýzu investičních nástrojů.
    /// </summary>
    public class FundamentalData
    {
        public Guid Id { get; private set; }

        public Guid InvestmentInstrumentId { get; private set; }

        /// <summary>
        /// Navigační vlastnost na InvestmentInstrument.
        /// </summary>
        public InvestmentInstrument InvestmentInstrument { get; private set; }
        
        /// <summary>
        /// Pomocné property pro přístup k poslední ceně
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Pomocná hodnota pro přístup k poslední ceně
        /// </summary>
        public PriceMetrics PriceMetrics => new PriceMetrics(Price);

        // Valuační ukazatele (P/E, P/B, EV/EBITDA atd.)
        public ValuationMetrics Valuation { get; private set; }

        // Růstové ukazatele (včetně PredictedFCFGrowth a LongTermGrowthRate)
        public GrowthMetrics Growth { get; private set; }

        // Profitabilita (ROE, marže atd.)
        public ProfitabilityMetrics Profitability { get; private set; }

        // Finanční stabilita (Debt/Equity, Current Ratio, atd.)
        public StabilityMetrics Stability { get; private set; }

        // Dividendové metriky (včetně CurrentAnnualDividend)
        public DividendMetrics Dividend { get; private set; }

        // Tržní riziko a volatilita
        public MarketRiskMetrics MarketRisk { get; private set; }

        // Sentiment a analytické odhady
        public SentimentMetrics Sentiment { get; private set; }

        // Metriky pro Comparable Company Analysis
        public ComparableMetrics Comparable { get; private set; }

        // Výnosy (EPS a EBITDA)
        public EarningsMetrics Earnings { get; private set; }

        // Tržby (např. SalesPerShare)
        public RevenueMetrics Revenue { get; private set; }

        // Volné cash flow a související parametry
        public CashFlowMetrics CashFlow { get; private set; }

        // Náklady kapitálu, včetně WACC a požadované návratnosti vlastního kapitálu
        public CostOfCapitalMetrics CostOfCapital { get; private set; }

        protected FundamentalData() { }

        public FundamentalData(
            ValuationMetrics valuation,
            GrowthMetrics growth,
            ProfitabilityMetrics profitability,
            StabilityMetrics stability,
            DividendMetrics dividend,
            MarketRiskMetrics marketRisk,
            SentimentMetrics sentiment,
            ComparableMetrics comparable,
            EarningsMetrics earnings,
            RevenueMetrics revenue,
            CashFlowMetrics cashFlow,
            CostOfCapitalMetrics costOfCapital)
        {
            Id = Guid.NewGuid();
            Valuation = valuation;
            Growth = growth;
            Profitability = profitability;
            Stability = stability;
            Dividend = dividend;
            MarketRisk = marketRisk;
            Sentiment = sentiment;
            Comparable = comparable;
            Earnings = earnings;
            Revenue = revenue;
            CashFlow = cashFlow;
            CostOfCapital = costOfCapital;
        }
    }
}