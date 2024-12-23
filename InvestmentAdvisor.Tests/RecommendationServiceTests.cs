using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using Moq;

namespace Tests
{
    public class RecommendationServiceTests
    {
        private readonly Mock<IValuationService> _valuationServiceMock;
        private readonly RecommendationService _recommendationService;

        public RecommendationServiceTests()
        {
            _valuationServiceMock = new Mock<IValuationService>();
            _recommendationService = new RecommendationService(_valuationServiceMock.Object);
        }

        /// <summary>
        /// Test ověřuje, zda RecommendationService vrátí StrongBuy, pokud je vnitřní hodnota o více než 20 % nad tržní cenou.
        /// </summary>
        [Fact]
        public void GenerateRecommendation_IntrinsicMuchHigherThanMarket_StrongBuy()
        {
            // Arrange
            var instrument = CreateInstrumentWithPrice(100m);
            _valuationServiceMock.Setup(v => v.CalculateIntrinsicValue(instrument)).Returns(130m); // 30% nad 100

            // Act
            var rec = _recommendationService.GenerateRecommendation(instrument);

            // Assert
            Assert.Equal(RecommendationAction.StrongBuy, rec.Action);
            Assert.True(rec.Score == 1);
        }

        /// <summary>
        /// Test ověřuje scénář, kdy je vnitřní hodnota jen mírně nad tržní cenou (např. 5 %).
        /// Výsledkem by měla být hodnota "Accumulate".
        /// </summary>
        [Fact]
        public void GenerateRecommendation_IntrinsicSlightlyHigher_Accumulate()
        {
            // Arrange
            var instrument = CreateInstrumentWithPrice(100m);
            _valuationServiceMock.Setup(v => v.CalculateIntrinsicValue(instrument)).Returns(105m); // 5% nad 100

            // Act
            var rec = _recommendationService.GenerateRecommendation(instrument);

            // Assert
            Assert.Equal(RecommendationAction.Accumulate, rec.Action);
            Assert.Equal(3, rec.Score);
        }

        /// <summary>
        /// Scénář, kdy je vnitřní hodnota nižší o cca 15 % než tržní cena.
        /// To by mělo vést k doporučení "Sell".
        /// </summary>
        [Fact]
        public void GenerateRecommendation_IntrinsicLowerMinus15Percent_Sell()
        {
            // Arrange
            var instrument = CreateInstrumentWithPrice(100m);
            _valuationServiceMock.Setup(v => v.CalculateIntrinsicValue(instrument)).Returns(85m); // -15% oproti 100

            // Act
            var rec = _recommendationService.GenerateRecommendation(instrument);

            // Assert
            Assert.Equal(RecommendationAction.Sell, rec.Action);
            Assert.Equal(6, rec.Score);
        }

        /// <summary>
        /// Testuje, jak recommendation reaguje na vysokou betu (riziko).
        /// Očekáváme stejné doporučení, ale vyšší risk level.
        /// </summary>
        [Fact]
        public void GenerateRecommendation_HighBeta_UpdatesRiskLevelHigh()
        {
            // Arrange
            var instrument = CreateInstrumentWithPrice(100m, beta: 1.5m);
            _valuationServiceMock.Setup(v => v.CalculateIntrinsicValue(instrument)).Returns(130m);

            // Act
            var rec = _recommendationService.GenerateRecommendation(instrument);

            // Assert
            Assert.Equal("Vysoké", rec.RiskLevel);
        }

        private InvestmentInstrument CreateInstrumentWithPrice(decimal price, decimal beta = 1.0m)
        {
            var fundamentalData = new FundamentalData(
                valuation: new ValuationMetrics(10m, 2m, 8m, 7m, 2m, 10m),
                growth: new GrowthMetrics(0.05m, 0.05m, 0.05m, 0.05m, 0.02m, 0.05m, 0.03m),
                profitability: new ProfitabilityMetrics(0.15m, 0.10m, 0.40m, 0.20m, 0.10m),
                stability: new StabilityMetrics(0.5m, 1.5m, 1.2m, 4m),
                dividend: new DividendMetrics(0.03m, 0.5m, 0.02m, 2m),
                marketRisk: new MarketRiskMetrics(beta, 0.5m, 0.2m),
                sentiment: new SentimentMetrics(120m, "Buy", 0.8m),
                comparable: new ComparableMetrics(15m, 2m, 12m, 3m),
                earnings: new EarningsMetrics(5m, 500m),
                revenue: new RevenueMetrics(50m),
                cashFlow: new CashFlowMetrics(100m, 0.05m),
                costOfCapital: new CostOfCapitalMetrics(0.08m, 0.10m)
            );

            var instrument = new InvestmentInstrument("TEST", "Test Company");
            instrument.UpdateFundamentalData(fundamentalData);
            instrument.AddPriceData(new PriceData(DateTime.UtcNow, price, price, price, price, 1000000));
            return instrument;
        }
    }
}
