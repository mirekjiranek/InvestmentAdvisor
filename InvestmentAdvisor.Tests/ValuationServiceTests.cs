using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Adapters.APIs;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvestmentAdvisor.Tests
{
    public class ValuationServiceTests
    {
        private readonly ValuationService _valuationService;

        public ValuationServiceTests()
        {
            // V reálné implementaci byste mohli injectnout konkrétní strategie/parametry.
            _valuationService = new ValuationService();
        }

        /// <summary>
        /// Test ověřuje, že ValuationService korektně spočítá vnitřní hodnotu pro standardní validní vstup.
        /// Zde se testuje základní scénář - běžná firma s průměrnými metrikami.
        /// </summary>
        [Fact]
        public void CalculateIntrinsicValue_StandardInput_ReturnsReasonableValue()
        {
            // Arrange
            var instrument = CreateMockInstrument(
                currentFCF: 100m,
                predictedFCFGrowth: 0.05m,
                wacc: 0.08m,
                requiredRoe: 0.10m,
                dividend: 2m,
                dividendGrowth: 0.02m,
                pe: 15m,
                sectorPE: 14m
            );

            // Act
            var value = _valuationService.CalculateIntrinsicValue(instrument);

            // Assert
            // Očekáváme hodnotu v rozmezí - v praxi by to mohly být konkrétnější aserce 
            // (např. vnitřní hodnota > 0 a menší než nějaká horní mez).
            Assert.True(value > 0, "Hodnota musí být kladná");
            Assert.True(value < 20000, "Hodnota musí být realistická, ne extrémně vysoká");
        }

        /// <summary>
        /// Test ověřuje chování při velmi vysokých hodnotách FCF a velmi nízkém WACC.
        /// Očekáváme vysokou vnitřní hodnotu, ale stále v nějakém rozsahu.
        /// Cílem je otestovat robustnost výpočtu v extrémních podmínkách.
        /// </summary>
        [Fact]
        public void CalculateIntrinsicValue_HighGrowthLowWacc_ProducesHighIntrinsicValue()
        {
            // Arrange
            var instrument = CreateMockInstrument(
                currentFCF: 10000m,
                predictedFCFGrowth: 0.10m,
                wacc: 0.05m,
                requiredRoe: 0.09m,
                dividend: 0m,
                dividendGrowth: 0m,
                pe: 20m,
                sectorPE: 15m
            );

            // Act
            var value = _valuationService.CalculateIntrinsicValue(instrument);

            // Assert
            // Očekáváme výrazně vyšší hodnotu vzhledem k nízkému WACC a vysokému FCF.
            Assert.True(value > 500000, "Hodnota by měla být extrémně vysoká");
        }

        /// <summary>
        /// Test s téměř nulovým FCF a vysokým WACC - simulace problematické firmy.
        /// Výsledná vnitřní hodnota by měla být velmi nízká.
        /// </summary>
        [Fact]
        public void CalculateIntrinsicValue_NearZeroFcfHighWacc_ProducesVeryLowValue()
        {
            // Arrange
            var instrument = CreateMockInstrument(
                currentFCF: 1m,
                predictedFCFGrowth: 0.0m,
                wacc: 0.20m,
                requiredRoe: 0.20m,
                dividend: 0m,
                dividendGrowth: 0m,
                pe: 50m,
                sectorPE: 25m
            );

            // Act
            var value = _valuationService.CalculateIntrinsicValue(instrument);

            // Assert
            Assert.True(value < 10, "Hodnota by měla být téměř nulová");
        }

        private InvestmentInstrument CreateMockInstrument(
            decimal currentFCF,
            decimal predictedFCFGrowth,
            decimal wacc,
            decimal requiredRoe,
            decimal dividend,
            decimal dividendGrowth,
            decimal pe,
            decimal sectorPE)
        {
            var fundamentalData = new FundamentalData(
                valuation: new ValuationMetrics(PE: pe, PB: 2m, EV_EBITDA: 10m, EV_EBIT: 9m, PriceSales: 3m, PriceCashFlow: 15m),
                growth: new GrowthMetrics(
                    HistoricalEpsGrowth: 0.05m,
                    PredictedEpsGrowth: 0.05m,
                    RevenueGrowth: 0.05m,
                    ProfitGrowth: 0.05m,
                    DividendGrowth: dividendGrowth,
                    PredictedFCFGrowth: predictedFCFGrowth,
                    LongTermGrowthRate: 0.03m
                ),
                profitability: new ProfitabilityMetrics(0.15m, 0.10m, 0.40m, 0.20m, 0.10m),
                stability: new StabilityMetrics(0.5m, 1.5m, 1.2m, 4m),
                dividend: new DividendMetrics(DividendYield: 0.03m, DividendPayoutRatio: 0.5m, DividendGrowth: dividendGrowth, CurrentAnnualDividend: dividend),
                marketRisk: new MarketRiskMetrics(Beta: 1.0m, SharpeRatio: 0.5m, StandardDeviation: 0.2m),
                sentiment: new SentimentMetrics(ConsensusTargetPrice: 120m, AnalystRecommendation: "Buy", MediaSentimentScore: 0.8m),
                comparable: new ComparableMetrics(SectorAveragePE: sectorPE, SectorMedianPB: 2m, PeerEVEBITDA: 12m, SectorPriceSales: 3m),
                earnings: new EarningsMetrics(EPS: 5m, EBITDA: 500m),
                revenue: new RevenueMetrics(SalesPerShare: 50m),
                cashFlow: new CashFlowMetrics(CurrentFCF: currentFCF, ProjectedFCFGrowth: predictedFCFGrowth),
                costOfCapital: new CostOfCapitalMetrics(WACC: wacc, RequiredReturnOnEquity: requiredRoe)
            );

            var instrument = new InvestmentInstrument("TEST", "Test Company");
            instrument.UpdateFundamentalData(fundamentalData);
            instrument.AddPriceData(new PriceData(DateTime.UtcNow, 100, 110, 90, 100, 1000000));

            return instrument;
        }
    }
}
